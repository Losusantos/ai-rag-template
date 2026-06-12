using Microsoft.Data.Sqlite;

namespace AiRagTemplate.Rag;

/// <summary>
/// SQLite による永続ベクトルストア。
/// 検索時は全チャンクを読み出して C# 側でコサイン類似度を計算する素朴な実装。
/// 数百〜数千チャンク規模のサンプル/業務 PoC には十分。大規模化する場合は
/// sqlite-vec 拡張や専用ベクトル DB へ <see cref="IVectorStore"/> 実装を差し替える。
/// </summary>
public sealed class SqliteVectorStore : IVectorStore
{
    private readonly string _connectionString;
    private bool _initialized;

    public SqliteVectorStore(IConfiguration configuration, IHostEnvironment environment)
    {
        var configured = configuration["VectorStore:DatabasePath"];
        var path = ResolvePath(configured, environment.ContentRootPath);

        var directory = Path.GetDirectoryName(path);
        if (!string.IsNullOrEmpty(directory))
        {
            Directory.CreateDirectory(directory);
        }

        _connectionString = new SqliteConnectionStringBuilder
        {
            DataSource = path,
        }.ToString();
    }

    public async Task InitializeAsync(CancellationToken cancellationToken = default)
    {
        if (_initialized)
        {
            return;
        }

        await using var connection = new SqliteConnection(_connectionString);
        await connection.OpenAsync(cancellationToken);

        await using var command = connection.CreateCommand();
        command.CommandText =
            """
            CREATE TABLE IF NOT EXISTS chunks (
                id        TEXT PRIMARY KEY,
                source    TEXT NOT NULL,
                content   TEXT NOT NULL,
                embedding BLOB NOT NULL
            );
            """;
        await command.ExecuteNonQueryAsync(cancellationToken);

        _initialized = true;
    }

    public async Task<int> CountAsync(CancellationToken cancellationToken = default)
    {
        await InitializeAsync(cancellationToken);

        await using var connection = new SqliteConnection(_connectionString);
        await connection.OpenAsync(cancellationToken);

        await using var command = connection.CreateCommand();
        command.CommandText = "SELECT COUNT(*) FROM chunks;";
        var result = await command.ExecuteScalarAsync(cancellationToken);

        return Convert.ToInt32(result);
    }

    public async Task ClearAsync(CancellationToken cancellationToken = default)
    {
        await InitializeAsync(cancellationToken);

        await using var connection = new SqliteConnection(_connectionString);
        await connection.OpenAsync(cancellationToken);

        await using var command = connection.CreateCommand();
        command.CommandText = "DELETE FROM chunks;";
        await command.ExecuteNonQueryAsync(cancellationToken);
    }

    public async Task UpsertAsync(IEnumerable<DocumentChunk> chunks, CancellationToken cancellationToken = default)
    {
        await InitializeAsync(cancellationToken);

        await using var connection = new SqliteConnection(_connectionString);
        await connection.OpenAsync(cancellationToken);
        await using var transaction = (SqliteTransaction)await connection.BeginTransactionAsync(cancellationToken);

        await using var command = connection.CreateCommand();
        command.Transaction = transaction;
        command.CommandText =
            """
            INSERT INTO chunks (id, source, content, embedding)
            VALUES ($id, $source, $content, $embedding)
            ON CONFLICT(id) DO UPDATE SET
                source = excluded.source,
                content = excluded.content,
                embedding = excluded.embedding;
            """;

        var idParam = command.CreateParameter();
        idParam.ParameterName = "$id";
        var sourceParam = command.CreateParameter();
        sourceParam.ParameterName = "$source";
        var contentParam = command.CreateParameter();
        contentParam.ParameterName = "$content";
        var embeddingParam = command.CreateParameter();
        embeddingParam.ParameterName = "$embedding";
        command.Parameters.Add(idParam);
        command.Parameters.Add(sourceParam);
        command.Parameters.Add(contentParam);
        command.Parameters.Add(embeddingParam);

        foreach (var chunk in chunks)
        {
            idParam.Value = chunk.Id;
            sourceParam.Value = chunk.Source;
            contentParam.Value = chunk.Content;
            embeddingParam.Value = VectorSerializer.ToBytes(chunk.Embedding);
            await command.ExecuteNonQueryAsync(cancellationToken);
        }

        await transaction.CommitAsync(cancellationToken);
    }

    public async Task<IReadOnlyList<ScoredChunk>> SearchAsync(
        ReadOnlyMemory<float> queryEmbedding, int topK, CancellationToken cancellationToken = default)
    {
        await InitializeAsync(cancellationToken);

        await using var connection = new SqliteConnection(_connectionString);
        await connection.OpenAsync(cancellationToken);

        await using var command = connection.CreateCommand();
        command.CommandText = "SELECT id, source, content, embedding FROM chunks;";

        var scored = new List<ScoredChunk>();
        await using var reader = await command.ExecuteReaderAsync(cancellationToken);
        while (await reader.ReadAsync(cancellationToken))
        {
            var id = reader.GetString(0);
            var source = reader.GetString(1);
            var content = reader.GetString(2);
            var embedding = VectorSerializer.FromBytes(GetBytes(reader, 3));
            var score = VectorMath.CosineSimilarity(queryEmbedding.Span, embedding.Span);
            scored.Add(new ScoredChunk(new DocumentChunk(id, source, content, embedding), score));
        }

        return scored
            .OrderByDescending(s => s.Score)
            .Take(topK)
            .ToList();
    }

    private static byte[] GetBytes(SqliteDataReader reader, int ordinal)
    {
        using var stream = reader.GetStream(ordinal);
        using var memory = new MemoryStream();
        stream.CopyTo(memory);
        return memory.ToArray();
    }

    private static string ResolvePath(string? configured, string contentRoot)
    {
        if (string.IsNullOrWhiteSpace(configured))
        {
            return Path.Combine(contentRoot, "App_Data", "vectors.db");
        }

        return Path.IsPathRooted(configured)
            ? configured
            : Path.Combine(contentRoot, configured);
    }
}

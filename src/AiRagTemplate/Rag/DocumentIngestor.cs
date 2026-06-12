using Microsoft.Extensions.AI;

namespace AiRagTemplate.Rag;

/// <summary>
/// data ディレクトリの文書を読み、チャンク化し、埋め込みを生成して
/// ベクトルストアへ格納する。
/// </summary>
public sealed class DocumentIngestor
{
    private static readonly string[] SupportedExtensions = [".md", ".txt"];

    private readonly IEmbeddingGenerator<string, Embedding<float>> _embeddingGenerator;
    private readonly IVectorStore _vectorStore;
    private readonly IHostEnvironment _environment;
    private readonly ILogger<DocumentIngestor> _logger;

    public DocumentIngestor(
        IEmbeddingGenerator<string, Embedding<float>> embeddingGenerator,
        IVectorStore vectorStore,
        IHostEnvironment environment,
        ILogger<DocumentIngestor> logger)
    {
        _embeddingGenerator = embeddingGenerator;
        _vectorStore = vectorStore;
        _environment = environment;
        _logger = logger;
    }

    /// <summary>
    /// 取り込みを実行し、格納したチャンク数を返す。
    /// <paramref name="reset"/> が true なら既存データを消してから入れ直す。
    /// </summary>
    public async Task<int> IngestAsync(bool reset, CancellationToken cancellationToken = default)
    {
        var dataDirectory = Path.Combine(_environment.ContentRootPath, "data");
        if (!Directory.Exists(dataDirectory))
        {
            _logger.LogWarning("data ディレクトリが見つかりません: {Path}", dataDirectory);
            return 0;
        }

        if (reset)
        {
            await _vectorStore.ClearAsync(cancellationToken);
        }

        var files = Directory
            .EnumerateFiles(dataDirectory, "*.*", SearchOption.AllDirectories)
            .Where(IsSupported)
            .OrderBy(f => f, StringComparer.Ordinal)
            .ToList();

        var allChunks = new List<DocumentChunk>();

        foreach (var file in files)
        {
            var source = Path.GetFileName(file);
            var content = await File.ReadAllTextAsync(file, cancellationToken);
            var pieces = TextChunker.Chunk(content);
            if (pieces.Count == 0)
            {
                continue;
            }

            // ファイル単位でまとめて埋め込みを生成 (API 呼び出し回数を削減)。
            var embeddings = await _embeddingGenerator.GenerateAsync(pieces, cancellationToken: cancellationToken);

            for (var i = 0; i < pieces.Count; i++)
            {
                allChunks.Add(new DocumentChunk(
                    Id: $"{source}#{i}",
                    Source: source,
                    Content: pieces[i],
                    Embedding: embeddings[i].Vector));
            }
        }

        await _vectorStore.UpsertAsync(allChunks, cancellationToken);
        _logger.LogInformation(
            "取り込み完了: {FileCount} ファイル / {ChunkCount} チャンク", files.Count, allChunks.Count);

        return allChunks.Count;
    }

    private static bool IsSupported(string path) =>
        SupportedExtensions.Contains(Path.GetExtension(path).ToLowerInvariant());
}

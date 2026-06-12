namespace AiRagTemplate.Rag;

/// <summary>
/// ベクトルストアの抽象。既定実装は <see cref="SqliteVectorStore"/>。
/// インメモリや外部ベクトル DB へ差し替える場合は、このインターフェースを実装して
/// Program.cs の登録を 1 行差し替えるだけでよい。
/// </summary>
public interface IVectorStore
{
    /// <summary>必要なテーブル等を用意する。複数回呼んでも安全。</summary>
    Task InitializeAsync(CancellationToken cancellationToken = default);

    /// <summary>格納済みチャンク数。</summary>
    Task<int> CountAsync(CancellationToken cancellationToken = default);

    /// <summary>全チャンクを削除する。</summary>
    Task ClearAsync(CancellationToken cancellationToken = default);

    /// <summary>チャンクを追加または更新する (Id 一致で上書き)。</summary>
    Task UpsertAsync(IEnumerable<DocumentChunk> chunks, CancellationToken cancellationToken = default);

    /// <summary>クエリ埋め込みに近い順で上位 <paramref name="topK"/> 件を返す。</summary>
    Task<IReadOnlyList<ScoredChunk>> SearchAsync(
        ReadOnlyMemory<float> queryEmbedding, int topK, CancellationToken cancellationToken = default);
}

namespace AiRagTemplate.Rag;

/// <summary>取り込んだ文書の 1 チャンクと、その埋め込みベクトル。</summary>
public sealed record DocumentChunk(
    string Id,
    string Source,
    string Content,
    ReadOnlyMemory<float> Embedding);

/// <summary>検索結果。類似度スコア付き。</summary>
public sealed record ScoredChunk(DocumentChunk Chunk, double Score);

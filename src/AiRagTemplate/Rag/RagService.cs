using System.Text;
using Microsoft.Extensions.AI;

namespace AiRagTemplate.Rag;

/// <summary>
/// RAG の中核。質問を埋め込み → 近傍チャンクを検索 → 文脈付きで LLM に問い合わせる。
/// 業務システムへ組み込む際は、このクラスを呼ぶだけで RAG 回答が得られる。
/// </summary>
public sealed class RagService
{
    private const int TopK = 4;

    private const string SystemPrompt =
        """
        あなたは社内ヘルプデスクのアシスタントです。以下のルールを厳守してください。
        - 回答は提供された「社内文書(抜粋)」の内容のみに基づいて作成する。
        - 文書に根拠がない場合は推測せず「提供された文書では分かりません」と答える。
        - 回答は日本語で簡潔に。可能なら根拠とした出典名を本文に示す。
        """;

    private readonly IChatClient _chatClient;
    private readonly IEmbeddingGenerator<string, Embedding<float>> _embeddingGenerator;
    private readonly IVectorStore _vectorStore;
    private readonly ILogger<RagService> _logger;

    public RagService(
        IChatClient chatClient,
        IEmbeddingGenerator<string, Embedding<float>> embeddingGenerator,
        IVectorStore vectorStore,
        ILogger<RagService> logger)
    {
        _chatClient = chatClient;
        _embeddingGenerator = embeddingGenerator;
        _vectorStore = vectorStore;
        _logger = logger;
    }

    public async Task<RagAnswer> AskAsync(string question, CancellationToken cancellationToken = default)
    {
        if (string.IsNullOrWhiteSpace(question))
        {
            throw new ArgumentException("質問が空です。", nameof(question));
        }

        var queryEmbeddings = await _embeddingGenerator.GenerateAsync(
            [question], cancellationToken: cancellationToken);
        var hits = await _vectorStore.SearchAsync(queryEmbeddings[0].Vector, TopK, cancellationToken);

        _logger.LogInformation("質問に対し {Count} 件の関連チャンクを取得しました。", hits.Count);

        var messages = new List<ChatMessage>
        {
            new(ChatRole.System, SystemPrompt),
            new(ChatRole.User, BuildUserPrompt(BuildContext(hits), question)),
        };

        var response = await _chatClient.GetResponseAsync(messages, cancellationToken: cancellationToken);
        var sources = hits
            .Select(h => h.Chunk.Source)
            .Distinct()
            .ToArray();

        return new RagAnswer(response.Text ?? string.Empty, sources);
    }

    private static string BuildContext(IReadOnlyList<ScoredChunk> hits)
    {
        if (hits.Count == 0)
        {
            return "(該当する社内文書は見つかりませんでした)";
        }

        var builder = new StringBuilder();
        foreach (var hit in hits)
        {
            builder.AppendLine($"[出典: {hit.Chunk.Source}]");
            builder.AppendLine(hit.Chunk.Content);
            builder.AppendLine();
        }

        return builder.ToString();
    }

    private static string BuildUserPrompt(string context, string question) =>
        $"""
        # 社内文書(抜粋)
        {context}

        # 質問
        {question}
        """;
}

/// <summary>RAG の回答本文と、参照した出典名。</summary>
public sealed record RagAnswer(string Answer, IReadOnlyList<string> Sources);

using AiRagTemplate.Rag;
using Microsoft.Extensions.AI;
using Microsoft.Extensions.Logging.Abstractions;
using Xunit;

namespace AiRagTemplate.Tests;

/// <summary>
/// RagService の出典付与ロジックを、外部 API(LLM/Embedding)を叩かずフェイクで検証する。
/// 「文書に根拠が無い回答には出典を付けない」振る舞いが中心。
/// </summary>
public class RagServiceTests
{
    [Fact]
    public async Task AskAsync_WhenModelReportsNotFound_ReturnsNoSources()
    {
        // 検索では文書がヒットしているが、モデルは「分かりません」と回答するケース。
        var store = new FakeVectorStore(
            Hit("A.md", "本文A", 0.9),
            Hit("B.md", "本文B", 0.8));
        var service = new RagService(
            new FakeChatClient("提供された文書では分かりません。"),
            new FakeEmbeddingGenerator(),
            store,
            NullLogger<RagService>.Instance);

        var answer = await service.AskAsync("無関係な質問");

        Assert.Empty(answer.Sources);
        Assert.Equal("提供された文書では分かりません。", answer.Answer);
    }

    [Fact]
    public async Task AskAsync_WhenModelAnswers_ReturnsDistinctSources()
    {
        // 同じファイル由来のチャンクが複数あっても、出典はファイル単位で重複排除される。
        var store = new FakeVectorStore(
            Hit("A.md", "本文A1", 0.9),
            Hit("A.md", "本文A2", 0.85),
            Hit("B.md", "本文B", 0.8));
        var service = new RagService(
            new FakeChatClient("締め日は毎月末日です。"),
            new FakeEmbeddingGenerator(),
            store,
            NullLogger<RagService>.Instance);

        var answer = await service.AskAsync("締め日は？");

        Assert.Equal("締め日は毎月末日です。", answer.Answer);
        Assert.Equal(new[] { "A.md", "B.md" }, answer.Sources);
    }

    [Fact]
    public async Task AskAsync_EmptyQuestion_Throws()
    {
        var service = new RagService(
            new FakeChatClient("x"),
            new FakeEmbeddingGenerator(),
            new FakeVectorStore(),
            NullLogger<RagService>.Instance);

        await Assert.ThrowsAsync<ArgumentException>(() => service.AskAsync("   "));
    }

    private static ScoredChunk Hit(string source, string content, double score) =>
        new(new DocumentChunk($"{source}#0", source, content, new float[] { 0.1f, 0.2f }), score);

    // ---- フェイク (外部 I/O を叩かない) ----

    private sealed class FakeChatClient : IChatClient
    {
        private readonly string _reply;

        public FakeChatClient(string reply) => _reply = reply;

        public Task<ChatResponse> GetResponseAsync(
            IEnumerable<ChatMessage> messages,
            ChatOptions? options = null,
            CancellationToken cancellationToken = default)
            => Task.FromResult(new ChatResponse(new ChatMessage(ChatRole.Assistant, _reply)));

        public IAsyncEnumerable<ChatResponseUpdate> GetStreamingResponseAsync(
            IEnumerable<ChatMessage> messages,
            ChatOptions? options = null,
            CancellationToken cancellationToken = default)
            => throw new NotSupportedException();

        public object? GetService(Type serviceType, object? serviceKey = null) => null;

        public void Dispose()
        {
        }
    }

    private sealed class FakeEmbeddingGenerator : IEmbeddingGenerator<string, Embedding<float>>
    {
        public Task<GeneratedEmbeddings<Embedding<float>>> GenerateAsync(
            IEnumerable<string> values,
            EmbeddingGenerationOptions? options = null,
            CancellationToken cancellationToken = default)
            => Task.FromResult(new GeneratedEmbeddings<Embedding<float>>(
                new[] { new Embedding<float>(new float[] { 0.1f, 0.2f, 0.3f }) }));

        public object? GetService(Type serviceType, object? serviceKey = null) => null;

        public void Dispose()
        {
        }
    }

    private sealed class FakeVectorStore : IVectorStore
    {
        private readonly IReadOnlyList<ScoredChunk> _hits;

        public FakeVectorStore(params ScoredChunk[] hits) => _hits = hits;

        public Task<IReadOnlyList<ScoredChunk>> SearchAsync(
            ReadOnlyMemory<float> queryEmbedding, int topK, CancellationToken cancellationToken = default)
            => Task.FromResult(_hits);

        public Task InitializeAsync(CancellationToken cancellationToken = default) => Task.CompletedTask;

        public Task<int> CountAsync(CancellationToken cancellationToken = default) => Task.FromResult(_hits.Count);

        public Task ClearAsync(CancellationToken cancellationToken = default) => Task.CompletedTask;

        public Task UpsertAsync(IEnumerable<DocumentChunk> chunks, CancellationToken cancellationToken = default)
            => Task.CompletedTask;
    }
}

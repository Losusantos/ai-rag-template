using Microsoft.Extensions.AI;

namespace AiRagTemplate.Ai;

/// <summary>
/// Azure OpenAI 未設定時に登録されるダミーの <see cref="IChatClient"/>。
/// 呼び出されると設定方法を案内する例外を投げる。
/// </summary>
internal sealed class NotConfiguredChatClient : IChatClient
{
    private const string Message =
        "Azure OpenAI が未設定です。AzureOpenAI:Endpoint / ApiKey / ChatDeployment / EmbeddingDeployment を "
        + "appsettings・ユーザーシークレット・環境変数のいずれかで設定してください (README 参照)。";

    public Task<ChatResponse> GetResponseAsync(
        IEnumerable<ChatMessage> messages,
        ChatOptions? options = null,
        CancellationToken cancellationToken = default)
        => throw new InvalidOperationException(Message);

    public IAsyncEnumerable<ChatResponseUpdate> GetStreamingResponseAsync(
        IEnumerable<ChatMessage> messages,
        ChatOptions? options = null,
        CancellationToken cancellationToken = default)
        => throw new InvalidOperationException(Message);

    public object? GetService(Type serviceType, object? serviceKey = null) => null;

    public void Dispose()
    {
    }
}

/// <summary>
/// Azure OpenAI 未設定時に登録されるダミーの埋め込み生成器。
/// </summary>
internal sealed class NotConfiguredEmbeddingGenerator : IEmbeddingGenerator<string, Embedding<float>>
{
    private const string Message =
        "Azure OpenAI が未設定です。AzureOpenAI:Endpoint / ApiKey / ChatDeployment / EmbeddingDeployment を "
        + "appsettings・ユーザーシークレット・環境変数のいずれかで設定してください (README 参照)。";

    public Task<GeneratedEmbeddings<Embedding<float>>> GenerateAsync(
        IEnumerable<string> values,
        EmbeddingGenerationOptions? options = null,
        CancellationToken cancellationToken = default)
        => throw new InvalidOperationException(Message);

    public object? GetService(Type serviceType, object? serviceKey = null) => null;

    public void Dispose()
    {
    }
}

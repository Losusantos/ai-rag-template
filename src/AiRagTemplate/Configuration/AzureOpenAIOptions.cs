namespace AiRagTemplate.Configuration;

/// <summary>
/// Azure OpenAI 接続設定。appsettings の "AzureOpenAI" セクション、
/// 環境変数 (AzureOpenAI__ApiKey 等)、ユーザーシークレットからバインドする。
/// </summary>
public sealed class AzureOpenAIOptions
{
    public const string SectionName = "AzureOpenAI";

    /// <summary>例: https://your-resource-name.openai.azure.com/</summary>
    public string Endpoint { get; init; } = string.Empty;

    /// <summary>API キー。コミットせずユーザーシークレット/環境変数で渡すこと。</summary>
    public string ApiKey { get; init; } = string.Empty;

    /// <summary>チャットモデルのデプロイ名 (例: gpt-5-mini)。</summary>
    public string ChatDeployment { get; init; } = "gpt-5-mini";

    /// <summary>Embedding モデルのデプロイ名 (例: text-embedding-3-small)。</summary>
    public string EmbeddingDeployment { get; init; } = "text-embedding-3-small";

    /// <summary>
    /// 実値が入っているか。プレースホルダ (&lt;...&gt;) や空値は未設定とみなす。
    /// 未設定でもアプリは起動させ、API 呼び出し時に分かりやすく失敗させる。
    /// </summary>
    public bool IsConfigured =>
        IsRealValue(Endpoint)
        && IsRealValue(ApiKey)
        && !string.IsNullOrWhiteSpace(ChatDeployment)
        && !string.IsNullOrWhiteSpace(EmbeddingDeployment)
        && Uri.TryCreate(Endpoint, UriKind.Absolute, out _);

    private static bool IsRealValue(string value) =>
        !string.IsNullOrWhiteSpace(value)
        && !value.Contains('<')
        && !value.Contains('>');
}

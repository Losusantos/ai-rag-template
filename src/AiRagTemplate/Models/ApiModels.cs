namespace AiRagTemplate.Models;

/// <summary>チャット要求 (フロントからの JSON)。</summary>
public sealed record ChatRequest(string Message);

/// <summary>チャット応答ペイロード。</summary>
public sealed record ChatResult(string Answer, IReadOnlyList<string> Sources);

/// <summary>
/// 共通 API レスポンスエンベロープ (プロジェクト規約)。
/// </summary>
public sealed record ApiResponse<T>(bool Success, T? Data, string? Error)
{
    public static ApiResponse<T> Ok(T data) => new(true, data, null);

    public static ApiResponse<T> Fail(string error) => new(false, default, error);
}

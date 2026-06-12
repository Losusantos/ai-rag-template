using AiRagTemplate.Models;
using AiRagTemplate.Rag;

namespace AiRagTemplate.Endpoints;

/// <summary>チャット / 取り込み用の Minimal API エンドポイント。</summary>
public static class ChatEndpoints
{
    public static void MapChatEndpoints(this WebApplication app)
    {
        // RAG チャット本体。
        app.MapPost("/api/chat", async (
            ChatRequest request, RagService ragService, CancellationToken cancellationToken) =>
        {
            if (request is null || string.IsNullOrWhiteSpace(request.Message))
            {
                return Results.BadRequest(ApiResponse<ChatResult>.Fail("メッセージが空です。"));
            }

            try
            {
                var answer = await ragService.AskAsync(request.Message, cancellationToken);
                return Results.Ok(ApiResponse<ChatResult>.Ok(
                    new ChatResult(answer.Answer, answer.Sources)));
            }
            catch (Exception ex)
            {
                app.Logger.LogError(ex, "チャット処理に失敗しました。");
                return Results.Json(
                    ApiResponse<ChatResult>.Fail(ToUserMessage(ex)),
                    statusCode: StatusCodes.Status500InternalServerError);
            }
        });

        // 文書の再取り込み (?reset=true で全消去してから入れ直す)。
        app.MapPost("/api/ingest", async (
            DocumentIngestor ingestor, bool? reset, CancellationToken cancellationToken) =>
        {
            try
            {
                var count = await ingestor.IngestAsync(reset ?? true, cancellationToken);
                return Results.Ok(ApiResponse<IngestResult>.Ok(new IngestResult(count)));
            }
            catch (Exception ex)
            {
                app.Logger.LogError(ex, "取り込みに失敗しました。");
                return Results.Json(
                    ApiResponse<IngestResult>.Fail(ToUserMessage(ex)),
                    statusCode: StatusCodes.Status500InternalServerError);
            }
        });
    }

    /// <summary>例外をユーザー向けメッセージへ変換する。詳細はログ側に残す。</summary>
    private static string ToUserMessage(Exception ex) =>
        ex is InvalidOperationException or ArgumentException
            ? ex.Message
            : "サーバー側でエラーが発生しました。ログを確認してください。";

    private sealed record IngestResult(int Chunks);
}

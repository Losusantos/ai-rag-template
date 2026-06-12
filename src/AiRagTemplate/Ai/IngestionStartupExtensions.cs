using AiRagTemplate.Configuration;
using AiRagTemplate.Rag;
using Microsoft.Extensions.Options;

namespace AiRagTemplate.Ai;

/// <summary>
/// 起動時のサンプル文書取り込みを担う拡張。
/// ベクトルストアが空のときだけ取り込み、二重投入を避ける。
/// </summary>
public static class IngestionStartupExtensions
{
    public static async Task EnsureDocumentsIngestedAsync(this IServiceProvider services)
    {
        using var scope = services.CreateScope();
        var provider = scope.ServiceProvider;

        var options = provider.GetRequiredService<IOptions<AzureOpenAIOptions>>().Value;
        var logger = provider.GetRequiredService<ILoggerFactory>().CreateLogger("Startup");

        if (!options.IsConfigured)
        {
            logger.LogWarning(
                "Azure OpenAI が未設定のため、起動時の文書取り込みをスキップしました。README の設定手順を参照してください。");
            return;
        }

        var vectorStore = provider.GetRequiredService<IVectorStore>();
        await vectorStore.InitializeAsync();

        var existing = await vectorStore.CountAsync();
        if (existing > 0)
        {
            logger.LogInformation("ベクトルストアに {Count} 件あるため取り込みをスキップしました。", existing);
            return;
        }

        var ingestor = provider.GetRequiredService<DocumentIngestor>();
        await ingestor.IngestAsync(reset: false);
    }
}

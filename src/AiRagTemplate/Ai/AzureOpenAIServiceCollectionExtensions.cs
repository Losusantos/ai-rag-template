using System.ClientModel;
using AiRagTemplate.Configuration;
using Azure.AI.OpenAI;
using Microsoft.Extensions.AI;

namespace AiRagTemplate.Ai;

/// <summary>
/// Azure OpenAI を裏に置いた <see cref="IChatClient"/> /
/// <see cref="IEmbeddingGenerator{TInput,TEmbedding}"/> を DI へ登録する。
/// </summary>
public static class AzureOpenAIServiceCollectionExtensions
{
    public static IServiceCollection AddAzureOpenAIClients(
        this IServiceCollection services, IConfiguration configuration)
    {
        var options = configuration
            .GetSection(AzureOpenAIOptions.SectionName)
            .Get<AzureOpenAIOptions>() ?? new AzureOpenAIOptions();

        if (!options.IsConfigured)
        {
            // 未設定でもアプリ自体は起動させ、実際の呼び出しで分かりやすく失敗させる。
            services.AddSingleton<IChatClient>(new NotConfiguredChatClient());
            services.AddSingleton<IEmbeddingGenerator<string, Embedding<float>>>(
                new NotConfiguredEmbeddingGenerator());
            return services;
        }

        var azureClient = new AzureOpenAIClient(
            new Uri(options.Endpoint), new ApiKeyCredential(options.ApiKey));

        // GetChatClient/GetEmbeddingClient はデプロイ名を受け取る。
        // AsIChatClient/AsIEmbeddingGenerator は Microsoft.Extensions.AI.OpenAI の拡張メソッド。
        services.AddSingleton<IChatClient>(
            azureClient.GetChatClient(options.ChatDeployment).AsIChatClient());
        services.AddSingleton<IEmbeddingGenerator<string, Embedding<float>>>(
            azureClient.GetEmbeddingClient(options.EmbeddingDeployment).AsIEmbeddingGenerator());

        return services;
    }
}

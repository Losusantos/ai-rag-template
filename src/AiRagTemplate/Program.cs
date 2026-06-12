using AiRagTemplate.Ai;
using AiRagTemplate.Configuration;
using AiRagTemplate.Endpoints;
using AiRagTemplate.Rag;

var builder = WebApplication.CreateBuilder(args);

// 設定バインド (appsettings + 環境変数 AzureOpenAI__* + ユーザーシークレット)
builder.Services.Configure<AzureOpenAIOptions>(
    builder.Configuration.GetSection(AzureOpenAIOptions.SectionName));

// Azure OpenAI を裏に置いた IChatClient / IEmbeddingGenerator を登録
// (未設定なら API 呼び出し時に分かりやすく失敗するスタブを登録し、UI は起動する)
builder.Services.AddAzureOpenAIClients(builder.Configuration);

// RAG 関連サービス
builder.Services.AddSingleton<IVectorStore, SqliteVectorStore>();
builder.Services.AddSingleton<DocumentIngestor>();
builder.Services.AddSingleton<RagService>();

var app = builder.Build();

// 単一 HTML ページ (wwwroot/index.html) を配信
app.UseDefaultFiles();
app.UseStaticFiles();

// 起動時、ベクトルストアが空ならサンプル文書を取り込む (Azure OpenAI 設定済みのときのみ)
await app.Services.EnsureDocumentsIngestedAsync();

app.MapChatEndpoints();

app.Run();

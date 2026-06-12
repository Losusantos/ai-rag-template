---
paths:
  - "**/*.cs"
---
# C#/.NET パターン

## 依存性注入

- 依存は `Program.cs` で登録し、コンストラクタ注入で受け取る(サービスロケータを避ける)。
- 抽象(インターフェース)に依存し、実装は登録の 1 行差し替えで交換可能にする。

```csharp
builder.Services.AddSingleton<IVectorStore, SqliteVectorStore>();
builder.Services.AddSingleton<RagService>();
```

## Repository / Store 抽象

- データアクセスは一貫したインターフェースの背後にカプセル化する。
- 標準操作を定義し、保存先の詳細(SQLite / インメモリ / 外部ベクトル DB)は具象実装に隠す。
- ビジネスロジックは保存メカニズムでなく抽象に依存する(`IVectorStore`)。

## API レスポンスエンベロープ

すべての API レスポンスを一貫した形にする。

```csharp
public sealed record ApiResponse<T>(bool Success, T? Data, string? Error)
{
    public static ApiResponse<T> Ok(T data) => new(true, data, null);
    public static ApiResponse<T> Fail(string error) => new(false, default, error);
}
```

## 設定 (Options パターン)

- 設定は強く型付けした Options クラスにバインドし、`IOptions<T>` で受け取る。
- プレースホルダ/未設定を検出するヘルパ(`IsConfigured`)を用意し、起動時に分かりやすく扱う。

```csharp
builder.Services.Configure<AzureOpenAIOptions>(
    builder.Configuration.GetSection(AzureOpenAIOptions.SectionName));
```

## Minimal API エンドポイント

- エンドポイントは機能ごとに拡張メソッドへ切り出す(`MapChatEndpoints`)。
- 入力検証 → 処理 → `ApiResponse<T>` で返却、例外は捕捉してユーザー向けメッセージへ変換。

## 結果と例外

- 想定内の失敗(検証エラー等)は結果型/`ApiResponse.Fail` で表現。
- 想定外は例外として投げ、境界(エンドポイント)で捕捉してログ + 安全なメッセージに変換する。

---
paths:
  - "**/*.cs"
---
# C#/.NET セキュリティ

## シークレット管理

- ソースや appsettings に実シークレットを書かない(プレースホルダのみ)。
- 実値は user-secrets / 環境変数(`AzureOpenAI__ApiKey` など)/ シークレットマネージャーで渡す。
- 起動時に必須設定の有無を検証し、無ければ分かりやすく失敗させる。

```csharp
// 良い例
var apiKey = options.ApiKey;
if (string.IsNullOrWhiteSpace(apiKey))
    throw new InvalidOperationException("AzureOpenAI:ApiKey が未設定です");

// 悪い例
var apiKey = "sk-1234567890abcdef"; // ハードコード禁止
```

## 入力検証

- システム境界(API 入力・外部データ・ファイル内容)で必ず検証してから処理する。
- 早期に失敗し、明確なメッセージを返す。外部データ(API レスポンス・モデル出力)を信頼しない。

```csharp
if (request is null || string.IsNullOrWhiteSpace(request.Message))
    return Results.BadRequest(ApiResponse<ChatResult>.Fail("メッセージが空です。"));
```

## SQL / データアクセス

- パラメータ化クエリのみを使う。ユーザー入力を文字列連結でクエリに入れない。

```csharp
command.CommandText = "SELECT * FROM chunks WHERE id = $id;";
command.Parameters.AddWithValue("$id", id); // パラメータ化
```

## 出力 / Web

- モデル出力や外部データを HTML として解釈させない(フロントは `textContent` で描画)。
- エラーメッセージに内部情報(スタックトレース・接続文字列・キー)を含めない。
- 例外詳細はサーバーログにのみ残し、ユーザーには安全な要約を返す。

## チェックリスト(コミット前)

- [ ] ハードコードされたシークレットがない
- [ ] すべてのユーザー入力が検証済み
- [ ] パラメータ化クエリのみ
- [ ] エラーメッセージが機密データを漏らさない
- [ ] 外部/モデル出力をサニタイズして描画している

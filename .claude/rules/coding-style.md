---
paths:
  - "**/*.cs"
---
# C#/.NET コーディングスタイル

## 不変性

- DTO・値オブジェクトは `record`(または `record struct`)を使う。
- フィールド/プロパティは可能な限り `readonly` / `init` にする。
- コレクションをその場で破壊的変更せず、新しいシーケンスを返す(LINQ の `Select`/`Where` 等)。

```csharp
// 良い例: 変更を加えた新インスタンスを返す
public sealed record DocumentChunk(string Id, string Source, string Content, ReadOnlyMemory<float> Embedding);

// 悪い例: 可変クラスを外から書き換える
public class Chunk { public string Content; } // フィールド公開・破壊的変更の温床
```

## 命名

- 型・メソッド・プロパティは PascalCase、ローカル変数・引数は camelCase、private フィールドは `_camelCase`。
- インターフェースは `I` プレフィックス(`IVectorStore`)。
- 非同期メソッドは `Async` サフィックス(`SearchAsync`)。
- 意味のある名前を付ける(`data`/`tmp`/`Manager` のような曖昧名を避ける)。

## ファイル構成

- 1 ファイル 1 主要型を基本とし、200-400行、最大800行。
- 機能/ドメイン単位で名前空間とフォルダを揃える(`AiRagTemplate.Rag` は `Rag/` 配下)。
- file-scoped namespace を使う(`namespace AiRagTemplate.Rag;`)。

## 非同期

- I/O は同期ブロッキング(`.Result` / `.Wait()`)せず `await` する。
- `CancellationToken` を引数の末尾で引き回し、ライブラリ呼び出しへ渡す。
- ライブラリ的コードでは `ConfigureAwait(false)` を検討(ASP.NET Core アプリ層では必須ではない)。

## その他

- `var` は右辺で型が明らかなときに使う。
- マジックナンバー/文字列は `const` か設定に切り出す。
- 深いネスト(4段以上)を避け、早期 return を使う。
- 絵文字を使わない。

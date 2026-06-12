---
paths:
  - "**/*.cs"
---
# C#/.NET テスト(xUnit)

## 最低カバレッジ: 80%

- ユニットテスト: 決定的ロジック(類似度計算・直列化・チャンク分割・Options 検証など)
- 統合テスト: エンドポイント・ストア操作(必要に応じて `WebApplicationFactory`)
- 外部 API(LLM/Embedding)は実呼び出しに依存させない(抽象をスタブ/フェイク化)

## テスト構成

- 1 つの振る舞いにつき 1 テスト。Arrange / Act / Assert を明確に分ける。
- テスト名は対象と期待を表す(`Method_Condition_ExpectedResult`)。
- `[Fact]` は単一ケース、`[Theory]` + `[InlineData]` は表形式の複数ケース。

```csharp
[Fact]
public void CosineSimilarity_IdenticalVectors_ReturnsOne()
{
    float[] a = [1f, 2f, 3f];
    float[] b = [1f, 2f, 3f];

    var score = VectorMath.CosineSimilarity(a, b);

    Assert.Equal(1d, score, precision: 6);
}
```

## TDD ワークフロー

1. 失敗するテストを書く(RED)
2. 最小限の実装で通す(GREEN)
3. テストを緑に保ったままリファクタリング(REFACTOR)
4. カバレッジ確認(80%+、重要ロジックは 100%)

## 実行

```bash
export DOTNET_ROOT="$HOME/.dotnet"; export PATH="$HOME/.dotnet:$PATH"
export DOTNET_SYSTEM_GLOBALIZATION_INVARIANT=1
dotnet test
```

## 方針

- 実装の詳細でなく「振る舞い」をテストする。
- 何でもモックせず、純粋ロジックは実物でテストし、外部 I/O のみ差し替える。
- テストが間違っている場合を除き、テストでなく実装を直す。

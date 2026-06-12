---
description: テスト駆動開発ワークフローを強制します。インターフェースをスキャフォールドし、最初にテストを生成し、次にテストに合格するための最小限のコードを実装します。80%以上のカバレッジを保証します。
---

# TDDコマンド(C#/.NET)

このコマンドは **tdd-guide** エージェントを呼び出し、テスト駆動開発(xUnit)を強制します。

## このコマンドの機能

1. **インターフェース/型のスキャフォールド** - まず型・シグネチャを定義
2. **最初にテストを生成** - 失敗するテストを書く(RED)
3. **最小限のコードを実装** - テストに合格するだけのコードを書く(GREEN)
4. **リファクタリング** - テストを緑に保ちながら改善(REFACTOR)
5. **カバレッジの検証** - 80%以上を保証

## 使用するタイミング

- 新機能の実装時 / 新しいクラス・関数の追加時
- バグ修正時(最初にバグを再現するテストを書く)
- 既存コードのリファクタリング時
- 重要なビジネスロジックの構築時

## TDDサイクル

```
RED → GREEN → REFACTOR → REPEAT

RED:      失敗するテストを書く
GREEN:    合格するための最小限のコードを書く
REFACTOR: コードを改善し、テストを合格状態に保つ
REPEAT:   次のシナリオへ
```

## 使用例

```
User: /tdd ベクトルのコサイン類似度を計算する関数が必要です
```

### ステップ1: シグネチャ定義(SCAFFOLD)

```csharp
// src/AiRagTemplate/Rag/VectorMath.cs
namespace AiRagTemplate.Rag;

public static class VectorMath
{
    public static double CosineSimilarity(ReadOnlySpan<float> a, ReadOnlySpan<float> b)
        => throw new NotImplementedException();
}
```

### ステップ2: 失敗するテストを書く(RED)

```csharp
// tests/AiRagTemplate.Tests/VectorMathTests.cs
using AiRagTemplate.Rag;
using Xunit;

public class VectorMathTests
{
    [Fact]
    public void IdenticalVectors_ReturnsOne()
    {
        float[] a = [1f, 2f, 3f];
        float[] b = [1f, 2f, 3f];

        Assert.Equal(1d, VectorMath.CosineSimilarity(a, b), precision: 6);
    }

    [Fact]
    public void OrthogonalVectors_ReturnsZero()
    {
        Assert.Equal(0d, VectorMath.CosineSimilarity([1f, 0f], [0f, 1f]), precision: 6);
    }

    [Fact]
    public void LengthMismatch_ReturnsZero()
        => Assert.Equal(0d, VectorMath.CosineSimilarity([1f, 2f, 3f], [1f, 2f]));
}
```

### ステップ3: テストを実行 - 失敗を確認

```bash
export DOTNET_ROOT="$HOME/.dotnet"; export PATH="$HOME/.dotnet:$PATH"
export DOTNET_SYSTEM_GLOBALIZATION_INVARIANT=1
dotnet test
# → NotImplementedException で失敗(期待通り)
```

### ステップ4: 最小限のコードを実装(GREEN)

```csharp
public static double CosineSimilarity(ReadOnlySpan<float> a, ReadOnlySpan<float> b)
{
    if (a.Length != b.Length || a.Length == 0) return 0d;

    double dot = 0d, normA = 0d, normB = 0d;
    for (var i = 0; i < a.Length; i++)
    {
        dot += a[i] * b[i];
        normA += a[i] * a[i];
        normB += b[i] * b[i];
    }

    if (normA == 0d || normB == 0d) return 0d;
    return dot / (Math.Sqrt(normA) * Math.Sqrt(normB));
}
```

### ステップ5: テストを実行 - 合格を確認 → ステップ6: リファクタリング → ステップ7: 再度合格を確認

リファクタリング後も `dotnet test` が緑であることを必ず確認する。

### ステップ8: カバレッジ確認

```bash
dotnet test --collect:"XPlat Code Coverage"
# 80% 未満なら不足ケース(エッジ・異常系)を追加
```

## TDDベストプラクティス

すべきこと:
- 実装の前にまずテストを書く
- 実装前にテストが失敗することを確認
- テストに合格する最小限のコードを書く
- 緑になってからのみリファクタリング
- エッジケース/異常系を追加し、80%以上(重要ロジックは100%)を目指す

してはいけないこと:
- テストの前に実装を書く
- 各変更後のテスト実行をスキップ
- 一度に大量のコードを書く
- 実装の詳細をテストする(振る舞いをテストする)
- 何でもモックする(純粋ロジックは実物でテスト)

## テストタイプ

- ユニット(xUnit `[Fact]`/`[Theory]`): 関数・決定的ロジック
- 統合: エンドポイント・ストア操作(必要に応じて `WebApplicationFactory`)
- 外部 API(LLM/Embedding): 抽象をスタブ/フェイク化し、実呼び出しに依存させない

## カバレッジ要件

- すべてのコードに 80%以上
- 100% 必須: 認証/認可ロジック、セキュリティクリティカルなコード、コアビジネスロジック

## 重要事項

必須: テストは実装の前に書く。RED → GREEN → REFACTOR のサイクルで、RED フェーズをスキップしない。

## 関連

- エージェント: `~/.claude/agents/tdd-guide.md`(無ければ汎用 tdd-guide)
- スキル: `.claude/skills/tdd-workflow/`
- 規約: `.claude/rules/testing.md`

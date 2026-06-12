namespace AiRagTemplate.Rag;

/// <summary>ベクトル類似度の計算。</summary>
public static class VectorMath
{
    /// <summary>
    /// コサイン類似度を返す (-1.0 〜 1.0)。長さ不一致や零ベクトルは 0.0。
    /// </summary>
    public static double CosineSimilarity(ReadOnlySpan<float> a, ReadOnlySpan<float> b)
    {
        if (a.Length != b.Length || a.Length == 0)
        {
            return 0d;
        }

        double dot = 0d;
        double normA = 0d;
        double normB = 0d;

        for (var i = 0; i < a.Length; i++)
        {
            dot += a[i] * b[i];
            normA += a[i] * a[i];
            normB += b[i] * b[i];
        }

        if (normA == 0d || normB == 0d)
        {
            return 0d;
        }

        return dot / (Math.Sqrt(normA) * Math.Sqrt(normB));
    }
}

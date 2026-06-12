using AiRagTemplate.Rag;
using Xunit;

namespace AiRagTemplate.Tests;

public class VectorMathTests
{
    [Fact]
    public void IdenticalVectors_ReturnsOne()
    {
        float[] a = [1f, 2f, 3f];
        float[] b = [1f, 2f, 3f];

        var score = VectorMath.CosineSimilarity(a, b);

        Assert.Equal(1d, score, precision: 6);
    }

    [Fact]
    public void OrthogonalVectors_ReturnsZero()
    {
        float[] a = [1f, 0f];
        float[] b = [0f, 1f];

        var score = VectorMath.CosineSimilarity(a, b);

        Assert.Equal(0d, score, precision: 6);
    }

    [Fact]
    public void OppositeVectors_ReturnsMinusOne()
    {
        float[] a = [1f, 1f];
        float[] b = [-1f, -1f];

        var score = VectorMath.CosineSimilarity(a, b);

        Assert.Equal(-1d, score, precision: 6);
    }

    [Fact]
    public void LengthMismatch_ReturnsZero()
    {
        float[] a = [1f, 2f, 3f];
        float[] b = [1f, 2f];

        var score = VectorMath.CosineSimilarity(a, b);

        Assert.Equal(0d, score);
    }

    [Fact]
    public void ZeroVector_ReturnsZero()
    {
        float[] a = [0f, 0f, 0f];
        float[] b = [1f, 2f, 3f];

        var score = VectorMath.CosineSimilarity(a, b);

        Assert.Equal(0d, score);
    }
}

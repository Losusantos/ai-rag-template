using AiRagTemplate.Rag;
using Xunit;

namespace AiRagTemplate.Tests;

public class VectorSerializerTests
{
    [Fact]
    public void RoundTrip_PreservesValues()
    {
        float[] original = [0.1f, -0.5f, 3.14159f, 0f, 12345.678f];

        var bytes = VectorSerializer.ToBytes(original);
        var restored = VectorSerializer.FromBytes(bytes);

        Assert.Equal(original, restored.ToArray());
    }

    [Fact]
    public void ToBytes_ProducesFourBytesPerFloat()
    {
        float[] original = [1f, 2f, 3f];

        var bytes = VectorSerializer.ToBytes(original);

        Assert.Equal(original.Length * sizeof(float), bytes.Length);
    }

    [Fact]
    public void EmptyVector_RoundTripsToEmpty()
    {
        var bytes = VectorSerializer.ToBytes(Array.Empty<float>());
        var restored = VectorSerializer.FromBytes(bytes);

        Assert.Empty(restored.ToArray());
    }
}

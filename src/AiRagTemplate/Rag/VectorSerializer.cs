using System.Buffers.Binary;

namespace AiRagTemplate.Rag;

/// <summary>
/// 埋め込みベクトル (float 配列) と SQLite BLOB の相互変換。
/// リトルエンディアン固定でプラットフォーム差を吸収する。
/// </summary>
public static class VectorSerializer
{
    public static byte[] ToBytes(ReadOnlyMemory<float> vector)
    {
        var span = vector.Span;
        var bytes = new byte[span.Length * sizeof(float)];
        for (var i = 0; i < span.Length; i++)
        {
            BinaryPrimitives.WriteSingleLittleEndian(bytes.AsSpan(i * sizeof(float)), span[i]);
        }

        return bytes;
    }

    public static ReadOnlyMemory<float> FromBytes(ReadOnlySpan<byte> bytes)
    {
        var count = bytes.Length / sizeof(float);
        var floats = new float[count];
        for (var i = 0; i < count; i++)
        {
            floats[i] = BinaryPrimitives.ReadSingleLittleEndian(bytes.Slice(i * sizeof(float)));
        }

        return floats;
    }
}

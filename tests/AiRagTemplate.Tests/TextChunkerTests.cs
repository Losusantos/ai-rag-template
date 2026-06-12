using AiRagTemplate.Rag;
using Xunit;

namespace AiRagTemplate.Tests;

public class TextChunkerTests
{
    [Fact]
    public void EmptyOrWhitespace_ReturnsEmpty()
    {
        Assert.Empty(TextChunker.Chunk(string.Empty));
        Assert.Empty(TextChunker.Chunk("   \n  "));
    }

    [Fact]
    public void ShortText_ReturnsSingleChunk()
    {
        var chunks = TextChunker.Chunk("短い文章です。", maxChars: 500);

        Assert.Single(chunks);
        Assert.Equal("短い文章です。", chunks[0]);
    }

    [Fact]
    public void LongText_SplitsIntoMultipleChunks()
    {
        var paragraph = new string('あ', 120);
        var text = string.Join("\n\n", Enumerable.Repeat(paragraph, 10));

        var chunks = TextChunker.Chunk(text, maxChars: 200, overlapChars: 20);

        Assert.True(chunks.Count > 1);
    }

    [Fact]
    public void OversizeParagraph_IsSplitByFixedLength()
    {
        var paragraph = new string('x', 1000);

        var chunks = TextChunker.Chunk(paragraph, maxChars: 200, overlapChars: 20);

        Assert.True(chunks.Count >= 5);
        Assert.All(chunks, chunk => Assert.True(chunk.Length <= 200));
    }
}

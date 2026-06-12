using System.Text;

namespace AiRagTemplate.Rag;

/// <summary>
/// 段落単位で結合しつつ最大長で分割する素朴なチャンカー。
/// 見出し + 段落構造を持つ業務文書 (規程・マニュアル・FAQ) に十分。
/// </summary>
public static class TextChunker
{
    public static IReadOnlyList<string> Chunk(string text, int maxChars = 500, int overlapChars = 80)
    {
        if (string.IsNullOrWhiteSpace(text))
        {
            return Array.Empty<string>();
        }

        var paragraphs = text
            .Replace("\r\n", "\n")
            .Split("\n\n", StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries);

        var chunks = new List<string>();
        var current = new StringBuilder();

        void Flush()
        {
            var trimmed = current.ToString().Trim();
            if (trimmed.Length > 0)
            {
                chunks.Add(trimmed);
            }

            current.Clear();
        }

        foreach (var paragraph in paragraphs)
        {
            // 1 段落だけで最大長を超える場合は固定長で割る。
            if (paragraph.Length > maxChars)
            {
                Flush();
                foreach (var slice in SplitFixed(paragraph, maxChars, overlapChars))
                {
                    chunks.Add(slice);
                }

                continue;
            }

            // 追記すると最大長を超えるなら、いったん確定し、末尾を重複として引き継ぐ。
            if (current.Length > 0 && current.Length + paragraph.Length + 2 > maxChars)
            {
                var tail = TakeTail(current.ToString(), overlapChars);
                Flush();
                if (tail.Length > 0)
                {
                    current.Append(tail).Append("\n\n");
                }
            }

            current.Append(paragraph).Append("\n\n");
        }

        Flush();
        return chunks;
    }

    private static string TakeTail(string text, int count)
    {
        var trimmed = text.Trim();
        return trimmed.Length <= count ? trimmed : trimmed[^count..];
    }

    private static IEnumerable<string> SplitFixed(string text, int maxChars, int overlap)
    {
        var step = Math.Max(1, maxChars - overlap);
        for (var start = 0; start < text.Length; start += step)
        {
            var length = Math.Min(maxChars, text.Length - start);
            yield return text.Substring(start, length).Trim();

            if (start + length >= text.Length)
            {
                yield break;
            }
        }
    }
}

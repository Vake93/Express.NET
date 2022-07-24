using CodeAnalysisTextSpan = Microsoft.CodeAnalysis.Text.TextSpan;

namespace Express.Net.CodeAnalysis.Text;

public record TextSpan(int Start, int Length)
{
    public static TextSpan Empty => new(0, 0);

    public int End => Start + Length;

    public static TextSpan FromCodeAnalysisTextSpan(CodeAnalysisTextSpan textSpan) => new(textSpan.Start, textSpan.Length);

    public static TextSpan FromBounds(int start, int end) => new(start, end - start);

    public bool OverlapsWith(TextSpan span) => Start < span.End && End > span.Start;

    public override string ToString() => $"{Start}..{End}";

    public static TextSpan operator +(TextSpan span1, TextSpan span2)
        => new(span1.Start + span2.Start, span1.Length);

    public static TextSpan operator -(TextSpan span1, TextSpan span2)
        => new(span1.Start - span2.Start, span1.Length);

    public static TextSpan operator +(TextSpan span, int offset)
        => new(span.Start + offset, span.Length);

    public static TextSpan operator -(TextSpan span, int offset)
        => new(span.Start - offset, span.Length);
}

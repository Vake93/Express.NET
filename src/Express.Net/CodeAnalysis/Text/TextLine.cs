using Microsoft.CodeAnalysis.Text;

namespace Express.Net.CodeAnalysis.Text;

public sealed class TextLine
{
    public TextLine(SourceText text, int start, int length, int lengthIncludingLineBreak)
    {
        Text = text;
        Start = start;
        Length = length;
        LengthIncludingLineBreak = lengthIncludingLineBreak;
    }

    public SourceText Text { get; init; }

    public int Start { get; init; }

    public int Length { get; init; }

    public int End => Start + Length;

    public int LengthIncludingLineBreak { get; init; }

    public int StartWhitespaceExcluding => Text.ToString(Span).TakeWhile(char.IsWhiteSpace).Count();

    public TextSpan Span => new(Start, Length);

    public TextSpan SpanIncludingLineBreak => new(Start, LengthIncludingLineBreak);

    public override string ToString() => Text.ToString(Span);
}

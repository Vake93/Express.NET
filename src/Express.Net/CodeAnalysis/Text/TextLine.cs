using System.Linq;

namespace Express.Net.CodeAnalysis.Text;

public record TextLine(SourceText Text, int Start, int Length, int LengthIncludingLineBreak)
{
    public int End => Start + Length;

    public int StartWhitespaceExcluding => Text.ToString(Span).TakeWhile(char.IsWhiteSpace).Count();

    public TextSpan Span => new(Start, Length);

    public TextSpan SpanIncludingLineBreak => new(Start, LengthIncludingLineBreak);

    public override string ToString() => Text.ToString(Span);
}
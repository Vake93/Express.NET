namespace Express.Net.CodeAnalysis.Text;

public record TextLocation(SourceText Text, TextSpan Span)
{
    public static TextLocation None => new(SourceText.Empty, TextSpan.Empty);

    public int StartLine => Text.GetLineIndex(Span.Start);

    public int StartCharacter => Span.Start - Text.Lines[StartLine].Start;

    public int EndLine => Text.GetLineIndex(Span.End);

    public int EndCharacter => Span.End - Text.Lines[EndLine].Start;

    public override string ToString() => Span.ToString();
}
namespace Express.Net.CodeAnalysis.Text
{
    public struct TextLocation
    {
        public TextLocation(SourceText text, TextSpan span)
        {
            Text = text;
            Span = span;
        }

        public SourceText Text { get; init; }

        public TextSpan Span { get; init; }

        public string FileName => Text.FileName;

        public int StartLine => Text.GetLineIndex(Span.Start);

        public int StartCharacter => Span.Start - Text.Lines[StartLine].Start;

        public int EndLine => Text.GetLineIndex(Span.End);

        public int EndCharacter => Span.End - Text.Lines[EndLine].Start;

        public override string ToString() => Span.ToString();
    }
}

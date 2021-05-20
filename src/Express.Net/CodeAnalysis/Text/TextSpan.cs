using CodeAnalysisTextSpan = Microsoft.CodeAnalysis.Text.TextSpan;

namespace Express.Net.CodeAnalysis.Text
{
    public struct TextSpan
    {
        public TextSpan(int start, int length)
        {
            Start = start;
            Length = length;
        }

        public int Start { get; init; }

        public int Length { get; init; }

        public int End => Start + Length;

        public static TextSpan FromCodeAnalysisTextSpan(CodeAnalysisTextSpan textSpan)
        {
            return new TextSpan(textSpan.Start, textSpan.Length);
        }

        public static TextSpan FromBounds(int start, int end)
        {
            var length = end - start;
            return new TextSpan(start, length);
        }

        public bool OverlapsWith(TextSpan span)
        {
            return Start < span.End &&
                End > span.Start;
        }

        public override string ToString() => $"{Start}..{End}";

        public static TextSpan operator +(TextSpan span1, TextSpan span2)
            => new(span1.Start + span2.Start, span1.Length);

        public static TextSpan operator -(TextSpan span1, TextSpan span2)
            => new(span1.Start - span2.Start, span1.Length);

        public static TextSpan operator +(TextSpan span, int offset)
            => new (span.Start + offset, span.Length);

        public static TextSpan operator -(TextSpan span, int offset)
            => new(span.Start - offset, span.Length);
    }
}

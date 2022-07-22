﻿using System.Collections.Immutable;
using System.Runtime.CompilerServices;
using System.Security.Cryptography;
using System.Text;

namespace Express.Net.CodeAnalysis.Text;

public sealed class SourceText
{
    private readonly string _text;
    private readonly byte[] _hash;

    private SourceText(string text)
    {
        _text = text;
        Lines = ParseLines(this, text);

        using var sha1 = SHA1.Create();
        _hash = sha1.ComputeHash(Encoding.UTF8.GetBytes(text));
    }

    public static SourceText Empty => new(string.Empty);

    public ImmutableArray<TextLine> Lines { get; init; }

    public char this[int index] => _text[index];

    public int Length => _text.Length;

    public byte[] Sha1Hash => _hash;

    public static SourceText From(string text) => new(text);

    public int GetLineIndex(int position)
    {
        var lower = 0;
        var upper = Lines.Length - 1;

        while (lower <= upper)
        {
            var index = lower + (upper - lower) / 2;
            var start = Lines[index].Start;

            if (position == start)
                return index;

            if (start > position)
            {
                upper = index - 1;
            }
            else
            {
                lower = index + 1;
            }
        }

        return lower - 1;
    }

    public override string ToString() => _text;

    public string ToString(int start, int length) => _text.Substring(start, length);

    public string ToString(TextSpan span) => ToString(span.Start, span.Length);

    private static ImmutableArray<TextLine> ParseLines(SourceText sourceText, string text)
    {
        var result = ImmutableArray.CreateBuilder<TextLine>();

        var position = 0;
        var lineStart = 0;

        while (position < text.Length)
        {
            var lineBreakWidth = GetLineBreakWidth(text, position);

            if (lineBreakWidth == 0)
            {
                position++;
            }
            else
            {
                AddLine(result, sourceText, position, lineStart, lineBreakWidth);

                position += lineBreakWidth;
                lineStart = position;
            }
        }

        if (position >= lineStart)
        {
            AddLine(result, sourceText, position, lineStart, 0);
        }

        return result.ToImmutable();

        static void AddLine(ImmutableArray<TextLine>.Builder result, SourceText sourceText, int position, int lineStart, int lineBreakWidth)
        {
            var lineLength = position - lineStart;
            var lineLengthIncludingLineBreak = lineLength + lineBreakWidth;
            var line = new TextLine(sourceText, lineStart, lineLength, lineLengthIncludingLineBreak);
            result.Add(line);
        }
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    private static int GetLineBreakWidth(string text, int position)
    {
        var c = text[position];
        var l = position + 1 >= text.Length ? '\0' : text[position + 1];

        if (c == '\r' && l == '\n')
            return 2;

        if (c == '\r' || c == '\n')
            return 1;

        return 0;
    }
}

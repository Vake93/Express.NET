using Express.Net.CodeAnalysis.Diagnostics;
using Express.Net.CodeAnalysis.Syntax.Nodes;
using Express.Net.CodeAnalysis.Text;
using System;
using System.Collections.Immutable;
using System.Text;

namespace Express.Net.CodeAnalysis.Syntax;

internal sealed class Lexer
{
    private readonly ImmutableArray<SyntaxTrivia>.Builder _triviaBuilder;
    private readonly SyntaxTree _syntaxTree;

    public Lexer(SyntaxTree syntaxTree)
    {
        _triviaBuilder = ImmutableArray.CreateBuilder<SyntaxTrivia>();
        _syntaxTree = syntaxTree;

        Diagnostics = new DiagnosticBag();
    }

    public DiagnosticBag Diagnostics { get; init; }

    private int Start { get; set; }

    private int Position { get; set; }

    private SyntaxKind Kind { get; set; }

    private object? Value { get; set; }

    private char Current => Peek();

    private char Lookahead => Peek(1);

    public SyntaxToken Lex()
    {
        ReadTrivia(leading: true);

        var leadingTrivia = _triviaBuilder.ToImmutable();
        var tokenStart = Position;

        ReadToken();

        var tokenKind = Kind;
        var tokenValue = Value;
        var tokenLength = Position - Start;

        ReadTrivia(leading: false);

        var trailingTrivia = _triviaBuilder.ToImmutable();

        var tokenText = _syntaxTree.Text.ToString(tokenStart, tokenLength);

        return new SyntaxToken(
            _syntaxTree,
            tokenKind,
            tokenStart,
            tokenText,
            tokenValue,
            leadingTrivia,
            trailingTrivia);
    }

    private char Peek(int offset = 0)
    {
        var index = Position + offset;

        if (index >= _syntaxTree.Text.Length)
        {
            return '\0';
        }

        return _syntaxTree.Text[index];
    }

    private void ReadTrivia(bool leading)
    {
        _triviaBuilder.Clear();

        var done = false;

        while (!done)
        {
            Start = Position;
            Kind = SyntaxKind.BadToken;
            Value = null;

            if (char.IsWhiteSpace(Current))
            {
                ReadWhiteSpace();
            }

            switch (Current)
            {
                //End of file
                case '\0':
                    done = true;
                    break;

                //Comments
                case '/' when Lookahead == '/':
                    ReadSingleLineComment();
                    break;

                case '/' when Lookahead == '*':
                    ReadMultiLineComment();
                    break;

                case '/':
                    done = true;
                    break;

                // Line Breaks
                case '\r':
                case '\n':
                    done = !leading;
                    ReadLineBreak();
                    break;

                // White Space
                case ' ':
                case '\t':
                    ReadWhiteSpace();
                    break;

                default:
                    done = true;
                    break;
            }

            var length = Position - Start;

            if (length > 0)
            {
                var text = _syntaxTree.Text.ToString(Start, length);
                _triviaBuilder.Add(new(Kind, Start, text));
            }
        }
    }

    private void ReadToken()
    {
        Start = Position;
        Kind = SyntaxKind.BadToken;
        Value = null;

        switch (Current)
        {
            case '\0':
                Kind = SyntaxKind.EndOfFileToken;
                break;

            case '(':
                Kind = SyntaxKind.OpenParenthesisToken;
                Position++;
                break;

            case ')':
                Kind = SyntaxKind.CloseParenthesisToken;
                Position++;
                break;

            case '<':
                Kind = SyntaxKind.LessThanToken;
                Position++;
                break;

            case '>':
                Kind = SyntaxKind.GreaterThanToken;
                Position++;
                break;

            case '[':
                Kind = SyntaxKind.OpenSquareBracket;
                Position++;
                break;

            case ']':
                Kind = SyntaxKind.CloseSquareBracket;
                Position++;
                break;

            case '|':
                Kind = SyntaxKind.PipeToken;
                Position++;
                break;

            case ',':
                Kind = SyntaxKind.CommaToken;
                Position++;
                break;

            case ';':
                Kind = SyntaxKind.SemicolonToken;
                Position++;
                break;

            case '?':
                Kind = SyntaxKind.QuestionMarkToken;
                Position++;
                break;

            case '.':
                Kind = SyntaxKind.DotToken;
                Position++;
                break;

            case '=':
                Kind = SyntaxKind.EqualsToken;
                Position++;
                break;

            case '{':
                ReadCodeBlock();
                break;

            case '0':
            case '1':
            case '2':
            case '3':
            case '4':
            case '5':
            case '6':
            case '7':
            case '8':
            case '9':
                ReadNumber();
                break;

            case '"':
                ReadString();
                break;

            default:
                ReadIdentifierOrKeyword();
                break;
        }
    }

    private void ReadWhiteSpace()
    {
        var done = false;

        while (!done)
        {
            if (!char.IsWhiteSpace(Current))
            {
                break;
            }

            switch (Current)
            {
                case '\0':
                case '\r':
                case '\n':
                    done = true;
                    break;

                default:
                    Position++;
                    break;
            }
        }

        Kind = SyntaxKind.WhitespaceTrivia;
    }

    private void ReadSingleLineComment()
    {
        Position += 2;
        var done = false;

        while (!done)
        {
            switch (Current)
            {
                case '\0':
                case '\r':
                case '\n':
                    done = true;
                    break;
                default:
                    Position++;
                    break;
            }
        }

        Kind = SyntaxKind.SingleLineCommentTrivia;
    }

    private void ReadMultiLineComment()
    {
        var done = false;
        Position += 2;

        while (!done)
        {
            switch (Current)
            {
                case '\0':
                    var span = new TextSpan(Start, 2);
                    var location = new TextLocation(_syntaxTree.Text, span);
                    Diagnostics.ReportUnterminatedMultiLineComment(location);
                    done = true;
                    break;

                case '*' when Lookahead == '/':
                    Position += 2;
                    done = true;
                    break;

                default:
                    Position++;
                    break;
            }
        }

        Kind = SyntaxKind.MultiLineCommentTrivia;
    }

    private void ReadLineBreak()
    {
        if (Current == '\r' && Lookahead == '\n')
        {
            Position += 2;
        }
        else
        {
            Position++;
        }

        Kind = SyntaxKind.LineBreakTrivia;
    }

    private void ReadCodeBlock()
    {
        var stringBuilder = new StringBuilder();
        var done = false;
        var bracketCount = 0;

        while (!done)
        {
            switch (Current)
            {
                case '{':
                    stringBuilder.Append(Current);
                    Position++;
                    bracketCount++;
                    done = bracketCount == 0;
                    break;

                case '}':
                    stringBuilder.Append(Current);
                    Position++;
                    bracketCount--;
                    done = bracketCount == 0;
                    break;

                case '\0':
                    var span = new TextSpan(Start, Position);
                    var location = new TextLocation(_syntaxTree.Text, span);
                    Diagnostics.ReportUnterminatedCodeBlock(location);
                    done = true;
                    break;

                default:
                    stringBuilder.Append(Current);
                    Position++;
                    break;
            }
        }

        Kind = SyntaxKind.CodeBlock;
        Value = stringBuilder.ToString();
    }

    private void ReadNumber()
    {
        while (char.IsDigit(Current))
            Position++;

        var length = Position - Start;
        var text = _syntaxTree.Text.ToString(Start, length);

        bool validNumber;
        object? value;
        Type? type;

        switch (Current)
        {
            case 'd':
                type = typeof(double);
                Position++;
                validNumber = double.TryParse(text, out var dbl);
                value = dbl;
                break;

            case 'f':
                type = typeof(float);
                Position++;
                validNumber = float.TryParse(text, out var flt);
                value = flt;
                break;

            case 'm':
                type = typeof(decimal);
                Position++;
                validNumber = float.TryParse(text, out var dec);
                value = dec;
                break;

            case 'l':
                type = typeof(long);
                Position++;
                validNumber = float.TryParse(text, out var lng);
                value = lng;
                break;

            case 'u' when Lookahead == 'l':
                type = typeof(ulong);
                Position += 2;
                validNumber = float.TryParse(text, out var uln);
                value = uln;
                break;

            case 'u':
                type = typeof(uint);
                Position++;
                validNumber = float.TryParse(text, out var uin);
                value = uin;
                break;

            default:
                type = typeof(int);
                validNumber = int.TryParse(text, out var @int);
                value = @int;
                break;
        }

        if (validNumber)
        {
            Value = value;
        }
        else
        {
            var span = new TextSpan(Start, length);
            var location = new TextLocation(_syntaxTree.Text, span);
            Diagnostics.ReportInvalidNumber(location, text, type);
        }

        Kind = SyntaxKind.NumberToken;
    }

    private void ReadString()
    {
        // Skip the current quote
        Position++;

        var stringBuilder = new StringBuilder();
        var done = false;

        while (!done)
        {
            switch (Current)
            {
                case '"':
                    Position++;
                    done = true;
                    break;

                case '\0':
                case '\r':
                case '\n':
                    var span = new TextSpan(Start, 1);
                    var location = new TextLocation(_syntaxTree.Text, span);
                    Diagnostics.ReportUnterminatedString(location);
                    done = true;
                    break;

                default:
                    stringBuilder.Append(Current);
                    Position++;
                    break;
            }
        }

        Kind = SyntaxKind.StringToken;
        Value = stringBuilder.ToString();
    }

    private void ReadIdentifierOrKeyword()
    {
        if (char.IsLetter(Current))
        {
            while (char.IsLetterOrDigit(Current))
            {
                Position++;
            }

            var length = Position - Start;
            var text = _syntaxTree.Text.ToString(Start, length);
            Kind = SyntaxFacts.GetKeywordKind(text);
        }
        else
        {
            var span = new TextSpan(Position, 1);
            var location = new TextLocation(_syntaxTree.Text, span);
            Diagnostics.ReportBadCharacter(location, Current);
            Position++;
        }
    }
}
using Express.Net.CodeAnalysis;
using Express.Net.CodeAnalysis.Syntax;
using Express.Net.CodeAnalysis.Syntax.Nodes;
using Express.Net.CodeAnalysis.Text;
using System;
using Xunit;

namespace Express.Net.Tests
{
    public class LexerTests
    {
        [Fact]
        public void UsingStatement()
        {
            var text = "//this is a using statement\r\nusing System.Linq;";

            var tokens = SyntaxTree.ParseTokens(text, out var diagnostics);

            Assert.Empty(diagnostics);

            Assert.Collection(tokens, new Action<SyntaxToken>[]
            {
                t =>
                {
                    Assert.Collection(t.LeadingTrivia, new Action<SyntaxTrivia>[]
                    {
                        lt =>
                        {
                            Assert.Equal(SyntaxKind.SingleLineCommentTrivia, lt.Kind);
                            Assert.Equal("//this is a using statement", lt.Text);
                        },
                        lt =>
                        {
                            Assert.Equal(SyntaxKind.LineBreakTrivia, lt.Kind);
                        }
                    });

                    Assert.Equal(SyntaxKind.UsingKeyword, t.Kind);

                    var trivia = Assert.Single(t.TrailingTrivia);
                    Assert.Equal(SyntaxKind.WhitespaceTrivia, trivia.Kind);

                    Assert.Equal("using", t.Text);
                },
                t =>
                {
                    Assert.Empty(t.LeadingTrivia);
                    Assert.Equal(SyntaxKind.IdentifierToken, t.Kind);
                    Assert.Empty(t.TrailingTrivia);
                    Assert.Equal("System", t.Text);
                },
                t =>
                {
                    Assert.Empty(t.LeadingTrivia);
                    Assert.Equal(SyntaxKind.DotToken, t.Kind);
                    Assert.Empty(t.TrailingTrivia);
                    Assert.Equal(".", t.Text);
                },
                t =>
                {
                    Assert.Empty(t.LeadingTrivia);
                    Assert.Equal(SyntaxKind.IdentifierToken, t.Kind);
                    Assert.Empty(t.TrailingTrivia);
                    Assert.Equal("Linq", t.Text);
                },
                t =>
                {
                    Assert.Empty(t.LeadingTrivia);
                    Assert.Equal(SyntaxKind.SemicolonToken, t.Kind);
                    Assert.Empty(t.TrailingTrivia);
                }
            });
        }

        [Fact]
        public void AliasUsingStatement()
        {
            var text = "//this is an alias using statement\r\nusing Foo = System;";

            var tokens = SyntaxTree.ParseTokens(text, out var diagnostics);

            Assert.Empty(diagnostics);

            Assert.Collection(tokens, new Action<SyntaxToken>[]
            {
                t =>
                {
                    Assert.Collection(t.LeadingTrivia, new Action<SyntaxTrivia>[]
                    {
                        lt =>
                        {
                            Assert.Equal(SyntaxKind.SingleLineCommentTrivia, lt.Kind);
                            Assert.Equal("//this is an alias using statement", lt.Text);
                        },
                        lt =>
                        {
                            Assert.Equal(SyntaxKind.LineBreakTrivia, lt.Kind);
                        }
                    });

                    Assert.Equal(SyntaxKind.UsingKeyword, t.Kind);

                    var trivia = Assert.Single(t.TrailingTrivia);
                    Assert.Equal(SyntaxKind.WhitespaceTrivia, trivia.Kind);

                    Assert.Equal("using", t.Text);
                },
                t =>
                {
                    Assert.Equal(SyntaxKind.IdentifierToken, t.Kind);
                    Assert.Equal("Foo", t.Text);
                },
                t =>
                {
                    Assert.Equal(SyntaxKind.EqualsToken, t.Kind);
                },
                t =>
                {
                    Assert.Equal(SyntaxKind.IdentifierToken, t.Kind);
                    Assert.Equal("System", t.Text);
                },
                t =>
                {
                    Assert.Equal(SyntaxKind.SemicolonToken, t.Kind);
                }
            });
        }

        [Fact]
        public void NumberToken()
        {
            var text = "//This is a number token\r\n2048";

            var tokens = SyntaxTree.ParseTokens(text, out var diagnostics);

            Assert.Empty(diagnostics);

            Assert.Collection(tokens, new Action<SyntaxToken>[]
            {
                t =>
                {
                    Assert.Collection(t.LeadingTrivia, new Action<SyntaxTrivia>[]
                    {
                        lt =>
                        {
                            Assert.Equal(SyntaxKind.SingleLineCommentTrivia, lt.Kind);
                            Assert.Equal("//This is a number token", lt.Text);
                        },
                        lt =>
                        {
                            Assert.Equal(SyntaxKind.LineBreakTrivia, lt.Kind);
                        }
                    });

                    Assert.Equal(SyntaxKind.NumberToken, t.Kind);
                    Assert.Equal(2048, t.Value);
                }
            });
        }

        [Fact]
        public void StringToken()
        {
            var text = "//This is a string token\r\n\"Hello World!\"";

            var tokens = SyntaxTree.ParseTokens(text, out var diagnostics);

            Assert.Empty(diagnostics);

            Assert.Collection(tokens, new Action<SyntaxToken>[]
            {
                t =>
                {
                    Assert.Collection(t.LeadingTrivia, new Action<SyntaxTrivia>[]
                    {
                        lt =>
                        {
                            Assert.Equal(SyntaxKind.SingleLineCommentTrivia, lt.Kind);
                            Assert.Equal("//This is a string token", lt.Text);
                        },
                        lt =>
                        {
                            Assert.Equal(SyntaxKind.LineBreakTrivia, lt.Kind);
                        }
                    });

                    Assert.Equal(SyntaxKind.StringToken, t.Kind);
                    Assert.Equal("\"Hello World!\"", t.Text);
                }
            });
        }

        [Fact]
        public void UnterminatedStringToken()
        {
            var text = "\"text";

            _ = SyntaxTree.ParseTokens(text, out var diagnostics);

            var diagnostic = Assert.Single(diagnostics);
            Assert.Equal(new TextSpan(0, 1), diagnostic.Location.Span);
            Assert.Equal("Unterminated string literal.", diagnostic.Message);
        }

        [Fact]
        public void Attribute()
        {
            var text = @"[Attribute(""Test"", testing = true)]";

            var tokens = SyntaxTree.ParseTokens(text, out var diagnostics);

            Assert.Empty(diagnostics);
            Assert.Equal(10, tokens.Length);
        }

        [Fact]
        public void CodeBlock()
        {
            var text = "//this is a code block\r\n{\r\nConsole.WriteLine(\"Test\");\r\n}";

            var tokens = SyntaxTree.ParseTokens(text, out var diagnostics);

            Assert.Empty(diagnostics);

            Assert.Collection(tokens, new Action<SyntaxToken>[]
            {
                t =>
                {
                    Assert.Collection(t.LeadingTrivia, new Action<SyntaxTrivia>[]
                    {
                        lt =>
                        {
                            Assert.Equal(SyntaxKind.SingleLineCommentTrivia, lt.Kind);
                            Assert.Equal("//this is a code block", lt.Text);
                        },
                        lt =>
                        {
                            Assert.Equal(SyntaxKind.LineBreakTrivia, lt.Kind);
                        }
                    });

                    Assert.Equal(SyntaxKind.CodeBlock, t.Kind);

                    Assert.Empty(t.TrailingTrivia);
                }
            });
        }

        [Fact]
        public void UnterminatedCodeBlock()
        {
            var text = "{Console.WriteLine(\"Test\");";

            _ = SyntaxTree.ParseTokens(text, out var diagnostics);

            var diagnostic = Assert.Single(diagnostics);
            Assert.Equal(new TextSpan(0, 27), diagnostic.Location.Span);
            Assert.Equal("Unterminated code block.", diagnostic.Message);
        }
    }
}

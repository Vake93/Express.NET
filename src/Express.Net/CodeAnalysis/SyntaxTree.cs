﻿using Express.Net.CodeAnalysis.Diagnostics;
using Express.Net.CodeAnalysis.Syntax;
using Express.Net.CodeAnalysis.Syntax.Nodes;
using Express.Net.CodeAnalysis.Text;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.IO;
using System.Threading;
using CSharpSyntaxTree = Microsoft.CodeAnalysis.SyntaxTree;

namespace Express.Net.CodeAnalysis
{
    public sealed class SyntaxTree
    {
        private Dictionary<SyntaxNode, SyntaxNode?>? _parents;

        private delegate void ParseHandler(SyntaxTree syntaxTree, out CompilationUnitSyntax root, out ImmutableArray<Diagnostic> diagnostics);

        private SyntaxTree(SourceText text, ParseHandler handler)
        {
            Text = text;

            handler(this, out var root, out var diagnostics);

            Diagnostics = new DiagnosticBag(diagnostics);
            Root = root;
        }

        public SourceText Text { get; init; }

        public DiagnosticBag Diagnostics { get; init; }

        public CompilationUnitSyntax Root { get; init; }

        public static SyntaxTree FromFile(string filepath)
        {
            var text = File.ReadAllText(filepath);
            return Parse(text);
        }

        public static SyntaxTree Parse(string text) => Parse(SourceText.From(text));

        public static SyntaxTree Parse(SourceText text) => new (text, Parse);

        public static ImmutableArray<SyntaxToken> ParseTokens(string text, out ImmutableArray<Diagnostic> diagnostics, bool includeEndOfFile = false)
        {
            var sourceText = SourceText.From(text);
            return ParseTokens(sourceText, out diagnostics, includeEndOfFile);
        }

        public static ImmutableArray<SyntaxToken> ParseTokens(SourceText text, out ImmutableArray<Diagnostic> diagnostics, bool includeEndOfFile = false)
        {
            var tokens = new List<SyntaxToken>();

            void ParseTokens(SyntaxTree st, out CompilationUnitSyntax root, out ImmutableArray<Diagnostic> d)
            {
                var l = new Lexer(st);
                while (true)
                {
                    var token = l.Lex();

                    if (token.Kind != SyntaxKind.EndOfFileToken || includeEndOfFile)
                        tokens.Add(token);

                    if (token.Kind == SyntaxKind.EndOfFileToken)
                    {
                        root = new CompilationUnitSyntax(st, ImmutableArray<MemberSyntax>.Empty, token);
                        break;
                    }
                }

                d = l.Diagnostics.ToImmutableArray();
            }

            var syntaxTree = new SyntaxTree(text, ParseTokens);
            diagnostics = syntaxTree.Diagnostics.ToImmutableArray();
            return tokens.ToImmutableArray();
        }

        public CSharpSyntaxTree Transform(string projectName, out DiagnosticBag diagnostics)
        {
            var transformer = new Transformer(this, projectName);
            var csharpSyntaxTree = transformer.Transform();

            diagnostics = transformer.Diagnostics;
            return csharpSyntaxTree;
        }

        private static void Parse(SyntaxTree syntaxTree, out CompilationUnitSyntax root, out ImmutableArray<Diagnostic> diagnostics)
        {
            var parser = new Parser(syntaxTree);
            root = parser.ParseCompilationUnit();
            diagnostics = parser.Diagnostics.ToImmutableArray();
        }

        internal SyntaxNode? GetParent(SyntaxNode syntaxNode)
        {
            if (_parents == null)
            {
                var parents = CreateParentsDictionary(Root);
                Interlocked.CompareExchange(ref _parents, parents, null);
            }

            return _parents[syntaxNode];
        }

        private Dictionary<SyntaxNode, SyntaxNode?> CreateParentsDictionary(CompilationUnitSyntax root)
        {
            var result = new Dictionary<SyntaxNode, SyntaxNode?>
            {
                { root, null }
            };
            CreateParentsDictionary(result, root);
            return result;
        }

        private void CreateParentsDictionary(Dictionary<SyntaxNode, SyntaxNode?> result, SyntaxNode node)
        {
            foreach (var child in node.GetChildren())
            {
                result.Add(child, node);
                CreateParentsDictionary(result, child);
            }
        }
    }
}

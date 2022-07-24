using Express.Net.CodeAnalysis.Text;
using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using CSharpSyntaxNode = Microsoft.CodeAnalysis.CSharp.CSharpSyntaxNode;

namespace Express.Net.CodeAnalysis.Syntax.Nodes;

public record CSharpBlockSyntax(
    SyntaxTree SyntaxTree,
    SyntaxToken CSharpKeyword,
    SyntaxToken CodeBlock,
    ImmutableArray<CSharpSyntax> Members) : MemberSyntax(SyntaxTree)
{
    public override SyntaxKind Kind { get; init; } = SyntaxKind.CSharpBlock;

    public override IEnumerable<SyntaxNode> GetChildren()
    {
        yield return CSharpKeyword;
        yield return CodeBlock;
    }

    public override string ToString() => "C# Code Block";
}

public record CSharpSyntax(
    SyntaxTree SyntaxTree,
    CSharpSyntaxNode CSharpStatement,
    TextSpan Span,
    TextSpan FullSpan) : SyntaxNode(SyntaxTree)
{
    public override SyntaxKind Kind { get; init; } = SyntaxKind.CSharpSyntax;

    public override TextSpan Span { get; } = Span;

    public override TextSpan FullSpan { get; } = FullSpan;

    public override IEnumerable<SyntaxNode> GetChildren() => Array.Empty<SyntaxNode>();

    public override string ToString() => "C# Statement";
}

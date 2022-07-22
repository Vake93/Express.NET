using System.Collections.Generic;
using System.Collections.Immutable;

namespace Express.Net.CodeAnalysis.Syntax.Nodes;

public sealed record CompilationUnitSyntax(
    SyntaxTree SyntaxTree,
    ImmutableArray<MemberSyntax> Members,
    SyntaxToken EndOfFileToken) : SyntaxNode(SyntaxTree)
{
    public override SyntaxKind Kind { get; init; } = SyntaxKind.CompilationUnit;

    public override IEnumerable<SyntaxNode> GetChildren()
    {
        foreach (var child in Members)
            yield return child;

        yield return EndOfFileToken;
    }

    public override string ToString() => "Compilation Unit";
}

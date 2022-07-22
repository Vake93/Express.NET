using System.Collections.Generic;
using System.Collections.Immutable;

namespace Express.Net.CodeAnalysis.Syntax.Nodes;

public record EndpointDeclarationSyntax(
    SyntaxTree SyntaxTree,
    SyntaxToken HttpVerbKeyword,
    SyntaxToken Route,
    EndpointReturnTypesSyntax ReturnTypes,
    EndpointParameterListSyntax ParametersList,
    SyntaxToken EndpointDeclarationBody,
    ImmutableArray<CSharpSyntax> Statements,
    bool Async = true,
    AttributeListSyntax? AttributeList = null) : MemberSyntax(SyntaxTree)
{
    public override SyntaxKind Kind { get; init; } = SyntaxKind.EndpointDeclaration;

    public override IEnumerable<SyntaxNode> GetChildren()
    {
        if (AttributeList is not null)
        {
            yield return AttributeList;
        }

        yield return HttpVerbKeyword;
        yield return Route;
        yield return ReturnTypes;
        yield return ParametersList;
        yield return EndpointDeclarationBody;
    }

    public override string ToString() => "Endpoint Declaration";
}

public record EndpointReturnTypesSyntax(
    SyntaxTree SyntaxTree,
    ImmutableArray<TypeClauseSyntax> Types) : MemberSyntax(SyntaxTree)
{
    public override SyntaxKind Kind { get; init; } = SyntaxKind.EndpointReturnType;

    public override IEnumerable<SyntaxNode> GetChildren() => Types;

    public override string ToString() => "Endpoint Return Types";
}

public record EndpointParameterListSyntax(
    SyntaxTree SyntaxTree,
    ImmutableArray<EndpointParameterSyntax> Parameters) : MemberSyntax(SyntaxTree)
{
    public override SyntaxKind Kind { get; init; } = SyntaxKind.EndpointParameterList;

    public override IEnumerable<SyntaxNode> GetChildren() => Parameters;

    public override string ToString() => "Endpoint Parameter List";
}

public record EndpointParameterSyntax(
    SyntaxTree SyntaxTree,
    SyntaxToken BindLocation,
    TypeClauseSyntax Type,
    SyntaxToken Identifer,
    SyntaxToken? DefaultValue = null,
    AttributeListSyntax? AttributeList = null) : MemberSyntax(SyntaxTree)
{
    public override SyntaxKind Kind { get; init; } = SyntaxKind.EndpointParameter;

    public override IEnumerable<SyntaxNode> GetChildren()
    {
        yield return BindLocation;
        yield return Type;
        yield return Identifer;

        if (DefaultValue is not null)
        {
            yield return DefaultValue;
        }
    }

    public override string ToString() => "Endpoint Parameter";
}
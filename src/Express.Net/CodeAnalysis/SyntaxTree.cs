using Express.Net.CodeAnalysis.Syntax.Nodes;
using Express.Net.CodeAnalysis.Text;

namespace Express.Net.CodeAnalysis;

public sealed class SyntaxTree
{
    private Dictionary<SyntaxNode, SyntaxNode?>? _parents;

    public string? FilePath { get; init; }

    public byte[] Sha1Hash => Text.Sha1Hash;

    public SourceText Text { get; init; }

    public static SyntaxTree FromFile(string filepath)
    {
        throw new NotImplementedException();
    }

    internal SyntaxNode? GetParent(SyntaxNode syntaxNode)
    {
        throw new NotImplementedException();
    }
}

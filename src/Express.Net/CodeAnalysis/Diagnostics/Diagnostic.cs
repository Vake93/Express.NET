using Express.Net.CodeAnalysis.Text;
using CSharpDiagnostic = Microsoft.CodeAnalysis.Diagnostic;

namespace Express.Net.CodeAnalysis.Diagnostics;

public sealed class Diagnostic
{
    private Diagnostic(DiagnosticType diagnosticType, TextLocation location, string message)
    {
        DiagnosticType = diagnosticType;
        Location = location;
        Message = message;
    }

    public DiagnosticType DiagnosticType { get; init; }

    public TextLocation Location { get; init; }

    public string Message { get; init; }

    public override string ToString() => $"{DiagnosticType}: {Message} @ {Location}";

    public static Diagnostic FromCSharpDiagnostic(CSharpDiagnostic diagnostic) => diagnostic.Severity switch
    {
        Microsoft.CodeAnalysis.DiagnosticSeverity.Error => Error(TextLocation.None, diagnostic.GetMessage()),
        Microsoft.CodeAnalysis.DiagnosticSeverity.Warning => Warning(TextLocation.None, diagnostic.GetMessage()),
        _ => Information(TextLocation.None, diagnostic.GetMessage()),
    };

    public static Diagnostic Error(TextLocation location, string message) => new(DiagnosticType.Error, location, message);

    public static Diagnostic Warning(TextLocation location, string message) => new(DiagnosticType.Warning, location, message);

    public static Diagnostic Information(TextLocation location, string message) => new(DiagnosticType.Infomation, location, message);
}

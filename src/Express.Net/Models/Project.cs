namespace Express.Net.Models
{
    public record Project(PackageReference[]? PackageReferences, LibraryReference[]? LibraryReferences);

    public record PackageReference(string Name, string Version);

    public record LibraryReference(string Path);
}

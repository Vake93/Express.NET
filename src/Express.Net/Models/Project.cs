namespace Express.Net.Models
{
    public record Project(PackageReference[]? PackageReferences, LibraryReference[]? LibraryReferences, bool GenerateSwaggerDoc, bool AddSwaggerUI);

    public record PackageReference(string Name, string Version);

    public record LibraryReference(string Path);
}

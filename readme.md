# Express.NET

Express.NET is a DSL for build fast web services effortlessly.
This builds on top of .NET / ASP.NET 5.

**This is my MSc Project.**

### Below is a simple Hello World Web service in Express.NET

```
service "hello" HelloWorldService;

get Ok ()
{
    return Ok("Hello World from Express.NET!");
}

get "{name}" Ok (route string name)
{
    return Ok($"Hello {name} from Express.NET!");
}
```

## Still this is work in progress. Things Todo

- [ ] Better diagnostic messages.
- [x] Add a concept of a project file.
- [ ] OpenAPI specification auto generation.
- [ ] Swagger support.
- [x] VSCode debugger using pdb files. 
- [ ] Write documentation on DSL syntax.
- [ ] Add a way to add NuGet packagers to a project.
- [ ] Easy database integrations.
- [ ] Target .NET 6.

## Attributions
- #### Roslyn
    - https://github.com/dotnet/roslyn
- #### Minsk
    - https://github.com/terrajobst/minsk
- #### Basic Reference Assemblies
    - https://github.com/jaredpar/basic-reference-assemblies
- #### Roslyn Quoter
    - https://github.com/KirillOsenkov/RoslynQuoter
- #### uController
    - https://github.com/davidfowl/uController

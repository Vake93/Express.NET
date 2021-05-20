# Express.NET

Express.NET is a DSL for build fast web services effortlessly.

**This is my MSc Project.**

### Below is a simple Hello World Web service in Express.NET.

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

## Still this is work in progress. Things Todo:

- Better diagnostic messages.
- Add a concept of a project file.
- Add a way to add NuGet packagers to a project.
- VSCode debugger.

## Attributions:
- ### Roslyn
    - https://github.com/dotnet/roslyn
- ### Minsk
    - https://github.com/terrajobst/minsk
- ### Basic Reference Assemblies
    - https://github.com/jaredpar/basic-reference-assemblies
- ### Roslyn Quoter
    - https://github.com/KirillOsenkov/RoslynQuoter
- ### uController
    - https://github.com/davidfowl/uController
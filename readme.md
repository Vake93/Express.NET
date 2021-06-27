# Express.NET

Express.NET is a DSL for build fast web services effortlessly.
This builds on top of .NET / ASP.NET 5.

**This is my MSc Project.**

# Hello world

The "Hello, World" program is traditionally used to introduce a programming language. Here it is in Express.NET:

```
service HelloWorldService;

get Ok ()
{
    return Ok("Hello World from Express.NET!");
}
```

To get started downloaded the xps binary from the releases section and run
```
xps new -n HelloWorld
xps run -i .\HelloWorld\
```

When the web service is started it should display the port the service is running on, this is usually port 5000 for HTTP and port 5001 for HTTPS.

![Express.NET Hello World](https://raw.githubusercontent.com/Vake93/Express.NET/main/doc/images/xps-helloworld.gif)

# Swagger at not extra code
Express.NET generates OpenAPI documents and has built-in swagger support. No extra code required!

```
service PersonService;

csharp
{
    public record Person (string Name, int Age);
    public record Error (string Description);
}

get Ok<Person> | NotFound<Error> (query string name)
{
    if (name == "Vishvaka")
        return Ok(new Person("Vishvaka", 28));
    else
        return NotFound(new Error($"Person with {name} not found."));
}
```
![Express.NET Swagger Support](https://raw.githubusercontent.com/Vake93/Express.NET/main/doc/images/xps-swagger.gif)

## Still this is work in progress. Things Todo

- [ ] Better diagnostic messages.
- [ ] Write documentation on DSL syntax.
- [ ] Hooks into service startup.
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

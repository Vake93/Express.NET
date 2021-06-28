# Express.NET

Express.NET is a lightweight, low-ceremony Domain Specific Language (DSL) for building HTTP based services easily. It is based-on .NET / ASP.NET 5. With the use of concise language constructs, this allows you to focus on important bits.. your code and your application! Express.NET handles **DELETE, GET, HEAD, POST, PUT** and **PATCH** requests and provides a simple, elegant syntax for returning a response with just a couple of keystrokes.

# Hello world

```csharp
service HelloWorldService;

get Ok ()
{
    return Ok("Hello World from Express.NET!");
}
```

To get started download the xps binary from the [releases section](https://github.com/Vake93/Express.NET/releases) and run
```
xps new -n HelloWorld
xps run -i .\HelloWorld\
```

When the web service is started it should display the port the service is running on, this is usually port 5000 for HTTP and port 5001 for HTTPS.

![Express.NET Hello World](https://raw.githubusercontent.com/Vake93/Express.NET/main/doc/images/xps-helloworld.gif)

# Swagger at not extra code
Express.NET generates OpenAPI documents and has built-in swagger support. No extra code required!

```csharp
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

# Consume NuGet Packages
Bring in your favorite NuGet packgers. 

Here is an example using ```StackExchange.Redis```

Project File:
```json
{
  "packageReferences": [
    {
      "name": "StackExchange.Redis",
      "version": "2.2.4"
    }
  ],
  "libraryReferences": [],
  "generateSwaggerDoc": true,
  "addSwaggerUI": true
}
```
Service File:
```csharp
using System;
using StackExchange.Redis;

service NuGetDemoService;

csharp
{
    private static ConnectionMultiplexer redis = ConnectionMultiplexer.Connect("localhost:6379");
}

get "ping" Ok<TimeSpan> ()
{
    var db = redis.GetDatabase();
    var pong = await db.PingAsync();
    return Ok(pong);
}
```
![Express.NET NuGet Support](https://raw.githubusercontent.com/Vake93/Express.NET/main/doc/images/xps-nuget.gif)

# Keep your flow with watch Command
With watch command Express.NET will re-build your project as and when the changers are saved.

Just run your project as
```
xps watch
```

![Express.NET watch](https://raw.githubusercontent.com/Vake93/Express.NET/main/doc/images/xps-watch.gif)

# Examples
More examples can be found [here](https://github.com/Vake93/Express.NET/tree/main/src/Examples)

# Still this is work in progress. Things Todo

- [ ] Better diagnostic messages.
- [ ] Write documentation on DSL syntax.
- [ ] Hooks into service startup.
- [ ] VS Code Extension.
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
# Express.NET

Express.NET is a DSL for build fast web services. \
This is my MSc Project.

*Huge disclaimer: I have no idea what I'm doing, and definitely shouldn't write languages.*

Below is a simple Hello World Web service in Express.NET.

```C#
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

### Still this is work in progress. Things Todo:

- Better diagnostic messages.
- Add a concept of a project file.
- Add a way to add NuGet packagers to a project.
- VSCode debugger.
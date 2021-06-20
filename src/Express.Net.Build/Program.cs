using Express.Net.Build.Commands;
using Spectre.Console;
using Spectre.Console.Cli;

var app = new CommandApp();

var rule = new Rule("Express.NET")
{
    Alignment = Justify.Center,
    Border = BoxBorder.Double,
    Style = Style.Parse("red"),
};

AnsiConsole.Render(rule);

app.Configure(config =>
{
    config.AddCommand<BuildCommand>("build");

    config.AddCommand<RunCommand>("run");

    config.AddCommand<NewCommand>("new");

    config.AddCommand<InspectCommand>("inspect");
});

app.Run(args);
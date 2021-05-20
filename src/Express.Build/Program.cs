using Express.Build.Commands;
using Spectre.Console.Cli;

var app = new CommandApp();

app.Configure(config =>
{
    config.AddCommand<BuildCommand>("build");

    config.AddCommand<RunCommand>("run");
});

app.Run(args);
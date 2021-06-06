using Express.Net.Build.Commands;
using Spectre.Console.Cli;

var app = new CommandApp();

app.Configure(config =>
{
    config.AddCommand<BuildCommand>("build");

    config.AddCommand<RunCommand>("run");

    config.AddCommand<NewCommand>("new");
});

app.Run(args);
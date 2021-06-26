using Express.Net.Build.Commands;
using Express.Net.Build.Services;
using Spectre.Console.Cli;

var app = new CommandApp();

Logger.WriteHeader();

app.Configure(config =>
{
    config.AddCommand<BuildCommand>("build");

    config.AddCommand<RunCommand>("run");

    config.AddCommand<NewCommand>("new");

    config.AddCommand<WatchCommand>("watch");

    config.AddCommand<InspectCommand>("inspect");
});

app.Run(args);
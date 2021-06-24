using Spectre.Console;
using System;

namespace Express.Net.Build.Services
{
    public static class Logger
    {
        public static void WriteLine(string message)
        {
            AnsiConsole.MarkupLine(Markup.Escape(message));
        }

        public static void LogInfo<T>(string message)
        {
            if (string.IsNullOrEmpty(message))
            {
                AnsiConsole.WriteLine();
                return;
            }

            var name = typeof(T).FullName;

            AnsiConsole.MarkupLine($"[bold green]info[/]: {name}");
            AnsiConsole.MarkupLine($"      {Markup.Escape(message)}");
        }

        public static void LogError<T>(string message)
        {
            if (string.IsNullOrEmpty(message))
            {
                AnsiConsole.WriteLine();
                return;
            }

            var name = typeof(T).FullName;

            AnsiConsole.MarkupLine($"[bold red]fail[/]: {name}");
            AnsiConsole.MarkupLine($"      {Markup.Escape(message)}");
        }

        public static void WriteException(Exception exception)
        {
            AnsiConsole.WriteException(exception);
        }
    }
}

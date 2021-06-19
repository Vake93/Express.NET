using Spectre.Console;
using System;

namespace Express.Net.Build.Services
{
    public static class Logger
    {
        public static void WriteLine(string message)
        {
            AnsiConsole.MarkupLine(message);
        }

        public static void LogInfo(string message)
        {
            if (string.IsNullOrEmpty(message))
            {
                AnsiConsole.WriteLine();
                return;
            }

            AnsiConsole.MarkupLine($"[bold green]info[/]: {message}");
        }

        public static void LogError(string message)
        {
            if (string.IsNullOrEmpty(message))
            {
                AnsiConsole.WriteLine();
                return;
            }

            AnsiConsole.MarkupLine($"[bold red]fail[/]: {message}");
        }

        public static void WriteException(Exception exception)
        {
            AnsiConsole.WriteException(exception);
        }
    }
}

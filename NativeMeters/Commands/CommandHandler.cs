using System;
using System.Collections.Generic;
using Dalamud.Game.Command;
using NativeMeters.Services;

namespace NativeMeters.Commands;

public class CommandHandler : IDisposable
{
    private const string MainCommand = "/nativemeters";
    private const string ShortCommand = "/nm";
    private const string HelpDescription = "Toggles your meters. Use '/nm help' for more options.";

    public CommandHandler()
    {
        Service.CommandManager.AddHandler(MainCommand, new CommandInfo(OnCommand)
        {
            DisplayOrder = 1,
            ShowInHelp = true,
            HelpMessage = HelpDescription
        });

        Service.CommandManager.AddHandler(ShortCommand, new CommandInfo(OnCommand)
        {
            DisplayOrder = 2,
            ShowInHelp = true,
            HelpMessage = HelpDescription
        });
    }

    private void OnCommand(string command, string args)
    {
        var argsParts = args.Trim().Split(' ', 2, StringSplitOptions.RemoveEmptyEntries);
        var subCommand = argsParts.Length > 0 ? argsParts[0].ToLowerInvariant() : string.Empty;
        var subArgs = argsParts.Length > 1 ? argsParts[1] : string.Empty;

        switch (subCommand)
        {
            case "":
            case "toggle":
                // TODO: Toggle Meters
                break;

            case "help":
            case "?":
                PrintHelp();
                break;

            default:
                PrintChat($"Unknown command: {subCommand}. Use '/ab help' for available commands.");
                break;
        }
    }

    private void PrintHelp()
    {
        var helpText = @"NativeMeters Commands:
  /nm              - Toggle meters
";
        PrintChat(helpText);
    }

    private static void PrintChat(string message)
    {
        Service.ChatGui.Print(message, "NativeMeters");
    }

    public void Dispose()
    {
        Service.CommandManager.RemoveHandler(MainCommand);
        Service.CommandManager.RemoveHandler(ShortCommand);
    }
}
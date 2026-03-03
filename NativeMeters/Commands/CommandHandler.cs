using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Dalamud.Game.Command;
using NativeMeters.Services;
using NativeMeters.Utilities;

namespace NativeMeters.Commands;

public class CommandHandler : IDisposable
{
    private const string MainCommand = "/nativemeters";
    private const string ShortCommand = "/ntm";

    private record SubCommand(Action<string> Action, string Description, string Usage = "");

    private readonly Dictionary<string, SubCommand> subCommands;

    public CommandHandler()
    {
        subCommands = new Dictionary<string, SubCommand>(StringComparer.OrdinalIgnoreCase)
        {
            ["config"] = new(args => System.AddonConfigurationWindow.Toggle(), "Open the configuration window"),
            ["toggle"] = new(args => {
                System.Config.General.IsEnabled = !System.Config.General.IsEnabled;
                System.OverlayManager.Setup();
                PrintChat($"Meters {(System.Config.General.IsEnabled ? "enabled" : "disabled")}.");
            }, "Toggle the meter display on/off"),
            ["breakdown"] = new(args => System.AddonDetailedBreakdownWindow.Toggle(), "Open the detailed breakdown window"),
            ["resetenmity"] = new(args => EnmityUtilities.ResetAllStrikingDummies(), "Resets enmity on all nearby Striking Dummies"),
            ["help"] = new(args => PrintHelp(), "Show this help message"),
        };

        var commandInfo = new CommandInfo(OnCommand)
        {
            DisplayOrder = 1,
            ShowInHelp = true,
            HelpMessage = "Main command for NativeMeters. Use '/ntm help' for all options."
        };

        Service.CommandManager.AddHandler(MainCommand, commandInfo);
        Service.CommandManager.AddHandler(ShortCommand, commandInfo);
    }

    private void OnCommand(string command, string args)
    {
        var parts = args.Trim().Split(' ', 2, StringSplitOptions.RemoveEmptyEntries);

        if (parts.Length == 0)
        {
            subCommands["config"].Action(string.Empty);
            return;
        }

        var subCommandName = parts[0].ToLowerInvariant();
        var subArgs = parts.Length > 1 ? parts[1] : string.Empty;

        if (subCommands.TryGetValue(subCommandName, out var sub))
        {
            sub.Action(subArgs);
        }
        else
        {
            PrintChat($"Unknown command: {subCommandName}. Type '{command} help' for a list of commands.");
        }
    }

    private void PrintHelp()
    {
        var sb = new StringBuilder("NativeMeters Help:\n");
        sb.AppendLine($"{ShortCommand} → Open configuration");

        foreach (var kvp in subCommands.OrderBy(x => x.Key))
        {
            if (kvp.Key == "help") continue;

            var usage = string.IsNullOrEmpty(kvp.Value.Usage) ? "" : $" {kvp.Value.Usage}";
            sb.AppendLine($"{ShortCommand} {kvp.Key}{usage} → {kvp.Value.Description}");
        }

        PrintChat(sb.ToString());
    }

    private static void PrintChat(string message)
    {
        Service.ChatGui.Print(message, "NativeMeters", 45);
    }

    public void Dispose()
    {
        Service.CommandManager.RemoveHandler(MainCommand);
        Service.CommandManager.RemoveHandler(ShortCommand);
    }
}

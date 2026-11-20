using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Dalamud.Game.Command;
using NativeMeters.Services;
using NativeMeters.Utilities;
using NativeMeters.Localization;

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
            ["config"] = new(args => System.AddonConfigurationWindow.Toggle(), Loc.Get("CommandOpenConfig")),
            ["setup"] = new(args =>
            {
                if (Service.ClientState.IsLoggedIn)
                    System.SetupWizard.OpenForSetup();
                else
                    PrintChat(Loc.Get("CommandLoginRequired"));
            }, Loc.Get("CommandOpenSetup")),
            ["toggle"] = new(args => {
                System.Config.General.IsEnabled = !System.Config.General.IsEnabled;
                System.OverlayManager.Setup();
                System.PartyListMeterManager?.UpdateSettings();
                PrintChat(Loc.Get(System.Config.General.IsEnabled ? "CommandMetersEnabled" : "CommandMetersDisabled"));
            }, Loc.Get("CommandToggle")),
            ["breakdown"] = new(args => System.AddonDetailedBreakdownWindow.Toggle(), Loc.Get("CommandOpenBreakdown")),
            ["resetenmity"] = new(args => EnmityUtilities.ResetAllStrikingDummies(), Loc.Get("CommandResetDummies")),
            ["help"] = new(args => PrintHelp(), Loc.Get("CommandShowHelp")),
        };

        var commandInfo = new CommandInfo(OnCommand)
        {
            DisplayOrder = 1,
            ShowInHelp = true,
            HelpMessage = Loc.Get("CommandMainHelp")
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

        if (subCommandName == "debug" && System.Config.General.DebugEnabled)
        {
            var debugArgs = subArgs.Split(' ', 2, StringSplitOptions.RemoveEmptyEntries);
            if (debugArgs.Length == 2 && debugArgs[0].Equals("capture", StringComparison.OrdinalIgnoreCase))
                Capture(debugArgs[1]);
            else
                PrintChat(Loc.Get("CommandCaptureUsage"));
            return;
        }

        if (subCommands.TryGetValue(subCommandName, out var sub))
        {
            sub.Action(subArgs);
        }
        else
        {
            PrintChat(Loc.Format("CommandUnknown", subCommandName, command));
        }
    }

    private static void Capture(string args)
    {
        try
        {
            switch (args.Trim().ToLowerInvariant())
            {
                case "start":
                    PrintChat(Loc.Format("CommandCaptureStarted", System.InternalMeterService.StartCapture()));
                    break;
                case "stop":
                    var path = System.InternalMeterService.StopCapture();
                    PrintChat(path == null ? Loc.Get("CommandCaptureNotRunning") : Loc.Format("CommandCaptureSaved", path));
                    break;
                default:
                    PrintChat(Loc.Get("CommandCaptureUsage"));
                    break;
            }
        }
        catch (Exception ex)
        {
            Service.Logger.Error(ex, "Parser capture failed.");
            PrintChat(Loc.Format("CommandCaptureFailed", ex.Message));
        }
    }

    private void PrintHelp()
    {
        var sb = new StringBuilder().AppendLine(Loc.Get("CommandHelpHeading"));
        sb.AppendLine($"{ShortCommand} → {Loc.Get("CommandOpenConfig")}");

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

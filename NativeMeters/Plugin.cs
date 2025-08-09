using Dalamud.Plugin;
using Dalamud.Game.Command;

namespace NativeMeters;

public class Plugin : IDalamudPlugin
{
    public const string CommandName = "/nativemeters";

    public Plugin(IDalamudPluginInterface pluginInterface)
    {
        pluginInterface.Create<Services>();

        Services.CommandManager.AddHandler(CommandName, new CommandInfo(this.OnCommand)
        {
            HelpMessage = ""
        });
    }

    public void Dispose()
    {
        Services.CommandManager.RemoveHandler(CommandName);
    }

    private void OnCommand(string command, string args)
    {
    }
}
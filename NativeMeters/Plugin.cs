using System;
using Dalamud.Plugin;
using Dalamud.Game.Command;
using Dalamud.Plugin.Services;
using FFXIVClientStructs.FFXIV.Client.Game.UI;
using FFXIVClientStructs.FFXIV.Client.System.Framework;
using FFXIVClientStructs.FFXIV.Client.UI;
using KamiToolKit;
using NativeMeters.Clients;
using NativeMeters.Services;
using StatusTimers.Helpers;

namespace NativeMeters;

public class Plugin : IDalamudPlugin
{
    public const string CommandName = "/nativemeters";

    public unsafe Plugin(IDalamudPluginInterface pluginInterface)
    {
        pluginInterface.Create<Service>();

        BackupHelper.DoConfigBackup(pluginInterface);

        Service.NativeController = new NativeController(pluginInterface);
        Service.NameplateAddonController = new NameplateAddonController(pluginInterface);

        Service.OverlayController = new OverlayController();
        Service.MeterService = new MeterService(new WebSocketClient(), new IINACTIpcClient());
        Service.TestMeterService = new TestMeterService();
        Service.ActiveMeterService = Service.MeterService;

        Service.CommandManager.AddHandler(CommandName, new CommandInfo(OnCommand)
        {
            HelpMessage = ""
        });

        if (Service.ClientState.IsLoggedIn) {
            Service.Framework.RunOnFrameworkThread(OnLogin);
        }



        Service.Framework.Update += OnFrameworkUpdate;
        Service.ClientState.Login += OnLogin;
        Service.ClientState.Logout += OnLogout;
        Service.NameplateAddonController.OnUpdate += OnNameplateUpdate;

    }

    public void Dispose()
    {
        Service.CommandManager.RemoveHandler(CommandName);

        Service.Framework.Update -= OnFrameworkUpdate;
        Service.ClientState.Login -= OnLogin;
        Service.ClientState.Logout -= OnLogout;

        Service.OverlayController.Dispose();
        Service.NativeController.Dispose();
        Service.NameplateAddonController.Dispose();
        Service.TestMeterService.Dispose();
        Service.MeterService.Dispose();
    }

    private void OnFrameworkUpdate(IFramework framework) {
        Service.MeterService.ProcessPendingMessages();
        //Service.Logger.Info(Service.MeterService.CurrentCombatData.Combatant.ToString());
    }

    private unsafe void OnNameplateUpdate(AddonNamePlate* nameplate) {
        Service.OverlayController.OnUpdate();
    }

    private void OnCommand(string command, string args) {
        //OverlayManager.ToggleConfig();
    }

    private void OnLogin() {
        Service.MeterService.Enable();
        Service.NameplateAddonController.Enable();

        #if DEBUG
            //OverlayManager.OpenConfig();
        #endif
    }

    private static void OnLogout(int type, int code) {
        Service.NameplateAddonController.Disable();
    }
}
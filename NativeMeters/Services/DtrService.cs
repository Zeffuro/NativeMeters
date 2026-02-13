using System;
using Dalamud.Game.Gui.Dtr;
using Dalamud.Game.Text.SeStringHandling;
using Dalamud.Plugin.Services;
using NativeMeters.Configuration;
using NativeMeters.Tags;

namespace NativeMeters.Services;

public class DtrService : IDisposable
{
    private IDtrBarEntry? dtrEntry;
    private DtrSettings Settings => System.Config.DtrSettings;

    private DateTime lastUpdate = DateTime.MinValue;

    public DtrService()
    {
        Service.Framework.Update += OnFrameworkUpdate;
    }

    private void OnFrameworkUpdate(IFramework framework)
    {
        if ((DateTime.Now - lastUpdate).TotalMilliseconds < 250) return;
        lastUpdate = DateTime.Now;

        UpdateBar();
    }

    public void UpdateBar()
    {
        if (!Settings.Enabled)
        {
            if (dtrEntry != null)
            {
                dtrEntry.OnClick -= OnDtrClick;
                dtrEntry.Remove();
                dtrEntry = null;
            }
            return;
        }

        if (dtrEntry == null)
        {
            dtrEntry = Service.DtrBar.Get("NativeMeters");
            if (dtrEntry != null)
            {
                dtrEntry.Shown = true;
                dtrEntry.OnClick += OnDtrClick;
            }
        }

        if (dtrEntry == null) return;

        if (!System.ActiveMeterService.IsConnected)
        {
            if (Settings.ShowWhenDisconnected)
            {
                dtrEntry.Text = new SeStringBuilder().AddText(Settings.DisconnectedText).Build();
                dtrEntry.Tooltip = "Not connected to ACT/IINACT";
                dtrEntry.Shown = true;
            }
            else
            {
                dtrEntry.Shown = false;
            }
            return;
        }

        dtrEntry.Shown = true;
        dtrEntry.Text = FormatDtrText();
        dtrEntry.Tooltip = BuildTooltip();
    }

    private SeString FormatDtrText()
    {
        if (!System.ActiveMeterService.HasCombatData())
            return new SeStringBuilder().AddText("Idle").Build();

        var combatant = System.ActiveMeterService.GetCombatant("YOU");
        var encounter = System.ActiveMeterService.GetEncounter();

        var format = Settings.FormatString;

        if (encounter != null) format = TagEngine.Process(format, encounter);
        if (combatant != null) format = TagEngine.Process(format, combatant);

        return new SeStringBuilder().AddText(format).Build();
    }

    private SeString BuildTooltip()
    {
        var builder = new SeStringBuilder();
        builder.AddUiForeground("NativeMeters", 540);

        var encounter = System.ActiveMeterService.GetEncounter();
        if (encounter != null)
        {
            builder.AddText($"\nEncounter: {encounter.Title}");
            builder.AddText($"\nDuration: {encounter.Duration:mm\\:ss}");
            builder.AddText($"\nRaid DPS: {encounter.ENCDPS:N0}");
            builder.AddText($"\nDeaths: {encounter.Deaths}");
        }

        if (Settings.ClickToOpenConfig)
            builder.AddUiForeground("\n\nClick to open config", 48);

        return builder.Build();
    }

    private void OnDtrClick(DtrInteractionEvent interaction)
    {
        if (Settings.ClickToOpenConfig)
        {
            System.AddonConfigurationWindow.Toggle();
        }
    }

    public void Dispose()
    {
        Service.Framework.Update -= OnFrameworkUpdate;

        if (dtrEntry != null)
        {
            dtrEntry.OnClick -= OnDtrClick;
            dtrEntry.Remove();
            dtrEntry = null;
        }
    }
}

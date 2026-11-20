using System;
using System.Collections.Generic;
using System.Linq;
using Dalamud.Game.Gui.Dtr;
using Dalamud.Game.Text.SeStringHandling;
using Dalamud.Plugin.Services;
using NativeMeters.Configuration;
using NativeMeters.Localization;
using NativeMeters.Models;
using NativeMeters.Tags;

namespace NativeMeters.Services;

public class DtrService : IDisposable
{
    private static readonly HashSet<string> PersonalDpsTags = new(StringComparer.OrdinalIgnoreCase)
    {
        "dps", "dpsk", "dpsm", "dpsstar",
    };

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
                dtrEntry.Tooltip = Loc.Get("HudDisconnected");
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
        var meterService = System.ActiveMeterService;
        if (!meterService.HasCombatData())
            return new SeStringBuilder().AddText(Loc.Get("HudIdle")).Build();

        var usePersonalDps = Settings.UsePersonalDps;
        var combatant = meterService.GetCombatant("YOU");
        if (usePersonalDps && (combatant == null || combatant.IsLocalInternalActor == false))
        {
            combatant = meterService is TestMeterService testMeterService
                ? testMeterService.GetSampleCombatant()
                : FindLocalCombatant(meterService);
        }

        var encounter = meterService.GetEncounter();
        return new SeStringBuilder().AddText(FormatStats(Settings.FormatString, encounter, combatant, usePersonalDps)).Build();
    }

    private static Combatant? FindLocalCombatant(IMeterService meterService)
    {
        var localName = Service.ObjectTable.LocalPlayer?.Name.TextValue;
        Combatant? namedCombatant = null;
        var ambiguousName = false;

        foreach (var combatant in meterService.GetCombatants())
        {
            if (combatant.IsLocalInternalActor is { } isLocalInternalActor)
            {
                if (isLocalInternalActor) return combatant;
                continue;
            }

            if (string.Equals(combatant.Name, "YOU", StringComparison.OrdinalIgnoreCase))
                return combatant;

            if (!string.IsNullOrWhiteSpace(localName) && string.Equals(combatant.Name, localName, StringComparison.OrdinalIgnoreCase))
            {
                ambiguousName |= namedCombatant != null;
                namedCombatant = combatant;
            }
        }

        return ambiguousName ? null : namedCombatant;
    }

    internal static string FormatStats(string format, Encounter? encounter, Combatant? combatant, bool usePersonalDps = false)
    {
        if (usePersonalDps) format = TagEngine.Process(format, encounter, ResolvePersonalDps);
        else if (encounter != null) format = TagEngine.Process(format, encounter);
        if (combatant != null) format = TagEngine.Process(format, combatant);
        return format;

        string? ResolvePersonalDps(ParsedTag tag)
        {
            if (!PersonalDpsTags.Contains(tag.Key)) return null;
            if (combatant == null) return "—";

            return TagEngine.FormatTag(tag, TagEngine.GetTagValue(tag, combatant), combatant);
        }
    }

    private SeString BuildTooltip()
    {
        var builder = new SeStringBuilder();
        builder.AddUiForeground("NativeMeters", 540);

        var encounter = System.ActiveMeterService.GetEncounter();
        if (encounter != null)
        {
            builder.AddText("\n" + Loc.Format("HudEncounterDetails", encounter.Title,
                encounter.Duration.ToString(@"mm\:ss"), encounter.ENCDPS, encounter.Deaths));

            if (System.ActiveMeterService.HasCombatData())
            {
                var combatants = System.ActiveMeterService.GetCombatants()
                    .OrderByDescending(c => c.ENCDPS)
                    .ToList();

                if (combatants.Count > 0)
                {
                    builder.AddText("\n");
                    foreach (var c in combatants)
                    {
                        if (c.Job.RowId == 0 && !c.Name.Equals("Limit Break", StringComparison.OrdinalIgnoreCase))
                            continue;

                        var name = c.Job.RowId != 0 ? c.Job.Abbreviation.ToString() : null;
                        if (string.IsNullOrEmpty(name))
                            name = c.Name.Equals("Limit Break", StringComparison.OrdinalIgnoreCase) ? Loc.Get("HudLimitBreak") : c.Name;

                        builder.AddText($"\n{name,-4} {c.ENCDPS:N0}");
                    }
                }
            }
        }

        if (Settings.ClickToOpenConfig)
            builder.AddUiForeground("\n\n" + Loc.Get("HudOpenConfig"), 48);

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

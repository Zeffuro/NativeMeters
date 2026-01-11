using System.Numerics;
using FFXIVClientStructs.FFXIV.Component.GUI;
using KamiToolKit.Classes;
using KamiToolKit.Nodes;
using NativeMeters.Extensions;
using NativeMeters.Helpers;
using NativeMeters.Models;
using NativeMeters.Services;

namespace NativeMeters.Nodes.LayoutNodes;

public sealed class MeterRowNode : SimpleComponentNode
{
    private ProgressBarCastNode barProgressNode;
    private IconImageNode iconImageNode;
    private TextNineGridNode nameTextNode;
    private TextNineGridNode dpsTextNode;

    public MeterRowNode()
    {
        X = 0;
        Y = 0;
        Width = 500;
        Height = 36;
        IsVisible = true;

        iconImageNode = new IconImageNode
        {
            NodeId = 2,
            X = 0,
            Y = 0,
            Width = 32,
            Height = 32,
            IsVisible = true,
            FitTexture = true
        };
        iconImageNode.AttachNode(this);

        barProgressNode = new ProgressBarCastNode
        {
            NodeId = 3,
            X = 32,
            Y = 10,
            Width = 200,
            Height = 20,
            IsVisible = true,
        };
        barProgressNode.AttachNode(this);

        nameTextNode = new TextNineGridNode()
        {
            NodeId = 4,
            X = 32,
            Y = 0,
            Width = 150,
            Height = 20,
            FontSize = 14,
            FontType = FontType.Axis,
            TextColor = ColorHelper.GetColor(50),
            TextOutlineColor = ColorHelper.GetColor(53),
            TextFlags = TextFlags.Edge,
            AlignmentType = AlignmentType.Left,
            IsVisible = true,
            String = Combatant?.Name ?? "Unknown",
        };
        nameTextNode.AttachNode(this);

        dpsTextNode = new TextNineGridNode()
        {
            NodeId = 5,
            X = 172,
            Y = 20,
            Width = 60,
            Height = 20,
            FontSize = 14,
            FontType = FontType.TrumpGothic,
            TextColor = ColorHelper.GetColor(50),
            TextOutlineColor = ColorHelper.GetColor(53),
            TextFlags = TextFlags.Edge,
            IsVisible = true,
            String = Combatant?.ENCDPS.ToString("0.00") ?? "0.00"
        };
        dpsTextNode.AttachNode(this);
    }

    public required Combatant Combatant
    {
        get;
        set
        {
            field = value;
            iconImageNode.IconId = Combatant.Job.GetIconId() ?? 0;
            nameTextNode.String = field.Name;
            dpsTextNode.String = field.ENCDPS.ToString("0.00");
            barProgressNode.BarColor = Combatant.Job.GetColor();
            Service.Logger.Info($"Set Combatant: {field.Name} with ENCDPS: {field.ENCDPS} and total: {Encounter?.ENCDPS} and Progress: {barProgressNode.Progress}");
        }
    }

    public required Encounter Encounter
    {
        get;
        set;
    }

    public void Update()
    {
        double maxEncdps = System.ActiveMeterService.GetMaxCombatantStat(c => c.ENCDPS);

        Combatant = System.ActiveMeterService.GetCombatant(Combatant.Name) ?? Combatant;
        Encounter = System.ActiveMeterService.GetEncounter() ?? Encounter;
        barProgressNode.Progress = MeterUtil.CalculateProgressRatio(Combatant.ENCDPS, maxEncdps > 0 ? maxEncdps : 1.0);
        Service.Logger.Info($"Set Combatant: {Combatant.Name} with ENCDPS: {Combatant.ENCDPS} and total: {Encounter?.ENCDPS} and Progress: {barProgressNode.Progress} and {Combatant.Job.GetColor()}");
    }
}
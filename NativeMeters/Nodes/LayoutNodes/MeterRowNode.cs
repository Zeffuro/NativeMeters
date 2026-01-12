using System.Numerics;
using FFXIVClientStructs.FFXIV.Component.GUI;
using KamiToolKit;
using KamiToolKit.Classes;
using KamiToolKit.Nodes;
using NativeMeters.Configuration;
using NativeMeters.Extensions;
using NativeMeters.Helpers;
using NativeMeters.Models;
using NativeMeters.Services;

namespace NativeMeters.Nodes.LayoutNodes;

public sealed class MeterRowNode : SimpleComponentNode
{
    // TODO: Allow swapping for other ProgressBarNodes
    private NodeBase? barProgressNode;
    private IconImageNode iconImageNode;
    private BackgroundTextNode nameTextNode;
    private BackgroundTextNode dpsTextNode;

    public MeterRowNode()
    {
        DisableCollisionNode = true;
        X = 0;
        Y = 0;
        Width = 500;
        Height = 36;
        IsVisible = true;

        iconImageNode = new IconImageNode
        {
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
            DisableCollisionNode = true,
            X = 32,
            Y = 10,
            Width = 200,
            Height = 20,
            IsVisible = true,
        };
        barProgressNode.AttachNode(this);

        nameTextNode = new BackgroundTextNode
        {
            X = 34,
            Y = 10,
            Padding = new Vector2(6, 2),
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
            DisableCollisionNode = true
        };
        nameTextNode.AttachNode(this);

        dpsTextNode = new BackgroundTextNode
        {
            X = 180,
            Y = 28,
            Padding = new Vector2(4, 1),
            Width = 60,
            Height = 20,
            FontSize = 14,
            FontType = FontType.TrumpGothic,
            TextColor = ColorHelper.GetColor(50),
            TextOutlineColor = ColorHelper.GetColor(53),
            TextFlags = TextFlags.Edge,
            IsVisible = true,
            ShowBackground = false,
            String = Combatant?.ENCDPS.ToString("0.00") ?? "0.00",
            DisableCollisionNode = true
        };
        dpsTextNode.AttachNode(this);
    }

    public required Combatant Combatant
    {
        get;
        set
        {
            field = value;
            if (MeterSettings == null) return;
            iconImageNode.IconId = field.GetIconId(MeterSettings.JobIconType);

            SetBarColor(field.GetColor());

            nameTextNode.String = field.Name;
            nameTextNode.ShowBackground = MeterSettings.BackgroundEnabled;
            dpsTextNode.String = field.ENCDPS.ToString("0.00");

            Service.Logger.DebugOnly($"Set Combatant: {field.Name} with ENCDPS: {field.ENCDPS}");
        }
    }

    public required Encounter Encounter
    {
        get;
        set;
    }

    public required MeterSettings MeterSettings
    {
        get;
        set
        {
            if (barProgressNode == null || field?.ProgressBarType != value.ProgressBarType)
            {
                field = value;
                RebuildProgressBar();
            }
            else
            {
                field = value;
            }
        }
    }

    private void RebuildProgressBar()
    {
        if (barProgressNode != null)
        {
            barProgressNode.DetachNode();
            barProgressNode.Dispose();
            barProgressNode = null;
        }

        barProgressNode = MeterSettings.ProgressBarType switch
        {
            ProgressBarType.Cast => new ProgressBarCastNode(),
            ProgressBarType.EnemyCast => new ProgressBarEnemyCastNode(),
            _ => new ProgressBarNode(),
        };

        barProgressNode.X = 32;
        barProgressNode.Y = 10;
        barProgressNode.Width = 200;
        barProgressNode.Height = 20;
        barProgressNode.IsVisible = true;

        barProgressNode.AttachNode(iconImageNode, NodePosition.BeforeTarget);
    }

    private void SetBarColor(Vector4 color)
    {
        if (barProgressNode == null) return;

        try
        {
            switch (barProgressNode)
            {
                case ProgressBarCastNode c:
                    c.BarColor = color;
                    break;
                case ProgressBarEnemyCastNode e:
                    e.BarColor = color;
                    break;
                case ProgressBarNode b:
                    b.BarColor = color;
                    break;
            }
        } catch { /* Ignored */ }
    }

    private void SetBarProgress(double progress)
    {
        if (barProgressNode == null) return;
        var barProgress = (float)progress;

        try
        {
            switch (barProgressNode)
            {
                case ProgressBarCastNode c:
                    c.Progress = barProgress;
                    break;
                case ProgressBarEnemyCastNode e:
                    e.Progress = barProgress;
                    break;
                case ProgressBarNode b:
                    b.Progress = barProgress;
                    break;
            }
        }
        catch { /* Ignored */ }
    }

    public void Update()
    {
        var selector = CombatantStatHelpers.GetStatSelector(MeterSettings.StatToTrack);
        double maxStat = System.ActiveMeterService.GetMaxCombatantStat(selector);
        Combatant = System.ActiveMeterService.GetCombatant(Combatant.Name) ?? Combatant;
        Encounter = System.ActiveMeterService.GetEncounter() ?? Encounter;

        double currentVal = selector(Combatant);
        var ratio = MeterUtil.CalculateProgressRatio(currentVal, maxStat > 0 ? maxStat : 1.0);
        SetBarProgress(ratio);

        dpsTextNode.String = CombatantStatHelpers.FormatStatValue(currentVal, MeterSettings.StatToTrack);
    }
}
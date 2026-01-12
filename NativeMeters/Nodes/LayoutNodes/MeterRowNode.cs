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
    private NodeBase barProgressNode = null!;
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
            Y = -2,
            Padding = new Vector2(6, 2),
            Width = 150,
            Height = 20,
            FontSize = 14,
            FontType = FontType.Axis,
            TextColor = ColorHelper.GetColor(50),
            TextOutlineColor = ColorHelper.GetColor(53),
            TextFlags = TextFlags.Edge | TextFlags.AutoAdjustNodeSize,
            AlignmentType = AlignmentType.Left,
            IsVisible = true,
            String = Combatant?.Name ?? "Unknown",
        };
        nameTextNode.CollisionNode.NodeFlags = 0;
        nameTextNode.AttachNode(this);

        dpsTextNode = new BackgroundTextNode
        {
            X = 180,
            Y = 18,
            Padding = new Vector2(4, 1),
            Width = 60,
            Height = 20,
            FontSize = 14,
            FontType = FontType.TrumpGothic,
            TextColor = ColorHelper.GetColor(50),
            TextOutlineColor = ColorHelper.GetColor(53),
            TextFlags = TextFlags.Edge | TextFlags.AutoAdjustNodeSize,
            IsVisible = true,
            ShowBackground = false,
            String = Combatant?.ENCDPS.ToString("0.00") ?? "0.00"
        };
        dpsTextNode.CollisionNode.NodeFlags = 0;
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
            var previousType = field?.ProgressBarType;
            field = value;

            // If the type changed (or this is the first set), rebuild the bar
            if (previousType != value.ProgressBarType)
            {
                RebuildProgressBar();
            }
        }
    }

    private void RebuildProgressBar()
    {
        barProgressNode.DetachNode();
        barProgressNode.Dispose();

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
        switch (barProgressNode)
        {
            case ProgressBarCastNode castBar:
                castBar.BarColor = color;
                break;
            case ProgressBarEnemyCastNode enemyCast:
                enemyCast.BarColor = color;
                break;
            case ProgressBarNode bar:
                bar.BarColor = color;
                break;
        }
    }

    private void SetBarProgress(double progress)
    {
        float barProgress = (float)progress;
        switch (barProgressNode)
        {
            case ProgressBarCastNode castBar:
                castBar.Progress = barProgress;
                break;
            case ProgressBarEnemyCastNode enemyCast:
                enemyCast.Progress = barProgress;
                break;
            case ProgressBarNode bar:
                bar.Progress = barProgress;
                break;
        }
    }

    public void Update()
    {
        double maxEncdps = System.ActiveMeterService.GetMaxCombatantStat(c => c.ENCDPS);

        Combatant = System.ActiveMeterService.GetCombatant(Combatant.Name) ?? Combatant;
        Encounter = System.ActiveMeterService.GetEncounter() ?? Encounter;

        var ratio = MeterUtil.CalculateProgressRatio(Combatant.ENCDPS, maxEncdps > 0 ? maxEncdps : 1.0);
        SetBarProgress(ratio);
        Service.Logger.DebugOnly($"Set Combatant: {Combatant.Name} with ENCDPS: {Combatant.ENCDPS} and total: {Encounter?.ENCDPS} and Ratio: {ratio} and {Combatant.Job.GetColor()}");
    }
}
using System;
using System.Numerics;
using KamiToolKit.Nodes;
using NativeMeters.Configuration;

namespace NativeMeters.Nodes.Components;

internal abstract class LegacyProgressBarNode<T> : ProgressNode, IMeterProgressNode where T : ProgressNode, new()
{
    public T BarNode { get; }
    private readonly NineGridNode fillNode;
    private float progress;
    private bool fillRightToLeft;

    protected LegacyProgressBarNode(Func<T, NineGridNode> getFillNode)
    {
        BarNode = new T();
        fillNode = getFillNode(BarNode);
        BarNode.AttachNode(this);
    }

    public override float Progress
    {
        get => progress;
        set
        {
            progress = float.IsFinite(value) ? Math.Clamp(value, 0, 1) : 0;
            ApplyProgress();
        }
    }

    public override Vector4 BarColor
    {
        get => BarNode.BarColor;
        set => BarNode.BarColor = value;
    }

    public override Vector4 BackgroundColor
    {
        get => BarNode.BackgroundColor;
        set => BarNode.BackgroundColor = value;
    }

    public ProgressBarColorTreatment ColorTreatment { get; set; }

    public bool FillRightToLeft
    {
        get => fillRightToLeft;
        set
        {
            if (fillRightToLeft == value) return;
            fillRightToLeft = value;
            ApplyProgress();
        }
    }

    protected override void OnSizeChanged()
    {
        base.OnSizeChanged();
        BarNode.Size = Size;
        ApplyProgress();
    }

    private void ApplyProgress()
    {
        var fillWidth = Width * progress;
        fillNode.Width = fillWidth;
        fillNode.X = fillRightToLeft ? Math.Max(0, Width - fillWidth) : 0;
    }
}

internal sealed class LegacyToDoProgressBarNode() : LegacyProgressBarNode<ProgressBarNode>(node => node.ForegroundNode);

internal sealed class LegacyCastProgressBarNode() : LegacyProgressBarNode<ProgressBarCastNode>(node => node.ProgressNode);

internal sealed class LegacyEnemyCastProgressBarNode() : LegacyProgressBarNode<ProgressBarEnemyCastNode>(node => node.ProgressNode);

using System;
using KamiToolKit.Nodes;
using NativeMeters.Configuration;

namespace NativeMeters.Nodes.Components;

internal sealed class LegacyToDoProgressBarNode : ProgressBarNode, IMeterProgressNode
{
    private float progress;
    private bool fillRightToLeft;

    public override float Progress
    {
        get => progress;
        set
        {
            progress = Math.Clamp(value, 0.0f, 1.0f);
            ApplyProgress();
        }
    }

    public ProgressBarColorTreatment ColorTreatment { get; set; } = ProgressBarColorTreatment.Auto;

    public bool FillRightToLeft
    {
        get => fillRightToLeft;
        set
        {
            if (fillRightToLeft == value)
                return;

            fillRightToLeft = value;
            ApplyProgress();
        }
    }

    protected override void OnSizeChanged()
    {
        base.OnSizeChanged();
        ApplyProgress();
    }

    private void ApplyProgress()
    {
        var fillWidth = Width * progress;
        ForegroundNode.Width = fillWidth;
        ForegroundNode.X = fillRightToLeft ? Math.Max(0.0f, Width - fillWidth) : 0.0f;
    }
}

internal sealed class LegacyCastProgressBarNode : ProgressBarCastNode, IMeterProgressNode
{
    private float progress;
    private bool fillRightToLeft;

    public override float Progress
    {
        get => progress;
        set
        {
            progress = Math.Clamp(value, 0.0f, 1.0f);
            ApplyProgress();
        }
    }

    public ProgressBarColorTreatment ColorTreatment { get; set; } = ProgressBarColorTreatment.Auto;

    public bool FillRightToLeft
    {
        get => fillRightToLeft;
        set
        {
            if (fillRightToLeft == value)
                return;

            fillRightToLeft = value;
            ApplyProgress();
        }
    }

    protected override void OnSizeChanged()
    {
        base.OnSizeChanged();
        ApplyProgress();
    }

    private void ApplyProgress()
    {
        var fillWidth = Width * progress;
        ProgressNode.Width = fillWidth;
        ProgressNode.X = fillRightToLeft ? Math.Max(0.0f, Width - fillWidth) : 0.0f;
    }
}

internal sealed class LegacyEnemyCastProgressBarNode : ProgressBarEnemyCastNode, IMeterProgressNode
{
    private float progress;
    private bool fillRightToLeft;

    public override float Progress
    {
        get => progress;
        set
        {
            progress = Math.Clamp(value, 0.0f, 1.0f);
            ApplyProgress();
        }
    }

    public ProgressBarColorTreatment ColorTreatment { get; set; } = ProgressBarColorTreatment.Auto;

    public bool FillRightToLeft
    {
        get => fillRightToLeft;
        set
        {
            if (fillRightToLeft == value)
                return;

            fillRightToLeft = value;
            ApplyProgress();
        }
    }

    protected override void OnSizeChanged()
    {
        base.OnSizeChanged();
        ApplyProgress();
    }

    private void ApplyProgress()
    {
        var fillWidth = Width * progress;
        ProgressNode.Width = fillWidth;
        ProgressNode.X = fillRightToLeft ? Math.Max(0.0f, Width - fillWidth) : 0.0f;
    }
}

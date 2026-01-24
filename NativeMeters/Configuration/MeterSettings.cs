using System;
using System.Collections.Generic;
using System.Numerics;
using NativeMeters.Models;

namespace NativeMeters.Configuration;

public class MeterSettings
{
    public string Id { get; set; } = Guid.NewGuid().ToString("N");
    public string Name { get; set; } = "New Meter";

    public bool IsEnabled { get; set; } = true;
    public bool IsLocked { get; set; } = false;
    public bool IsClickthrough { get; set; } = false;

    public Vector2 Position { get; set; } = new(500, 500);
    public Vector2 Size { get; set; } = new(250, 300);

    public float RowHeight { get; set; } = 36.0f;
    public float RowSpacing { get; set; } = 0.0f;

    public bool HeaderEnabled { get; set; } = true;
    public bool FooterEnabled { get; set; } = true;
    public float HeaderHeight { get; set; } = 28.0f;
    public float FooterHeight { get; set; } = 28.0f;

    public int MaxCombatants { get; set; } = 8;
    public bool ShowLimitBreak { get; set; } = true;
    public string StatToTrack { get; set; } = "ENCDPS";
    public JobIconType JobIconType { get; set; } = JobIconType.Default;
    public ProgressBarType ProgressBarType { get; set; } = ProgressBarType.Cast;
    public bool ShowWindowBackground { get; set; } = true;
    public Vector4 WindowColor { get; set; } = new(0, 0, 0, 0.5f);

    public List<ComponentSettings> RowComponents { get; set; } = [];
    public List<ComponentSettings> HeaderComponents { get; set; } = [];
    public List<ComponentSettings> FooterComponents { get; set; } = [];
}

public enum ProgressBarType
{
    Cast,
    EnemyCast,
    ToDo
}

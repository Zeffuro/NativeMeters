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

    public int MaxCombatants { get; set; } = 8;
    public bool ShowLimitBreak { get; set; } = true;
    public string StatToTrack { get; set; } = "ENCDPS";
    public JobIconType JobIconType { get; set; } = JobIconType.Default;
    public ProgressBarType ProgressBarType { get; set; } = ProgressBarType.Cast;
    public bool BackgroundEnabled { get; set; } = false;

    public List<RowComponentSettings> RowComponents { get; set; } = [];
    public List<RowComponentSettings> HeaderComponents { get; set; } = [];
    public List<RowComponentSettings> FooterComponents { get; set; } = [];
}

public enum ProgressBarType
{
    Cast,
    EnemyCast,
    ToDo
}
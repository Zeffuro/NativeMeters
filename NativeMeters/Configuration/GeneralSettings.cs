using System.Collections.Generic;
using System.Numerics;
using NativeMeters.Models;

namespace NativeMeters.Configuration;

public class GeneralSettings
{
    public bool DebugEnabled { get; set; } = false;
    public bool HideWithNativeUi { get; set; } = true;
    public bool IsEnabled { get; set; } = true;
    public bool PreviewEnabled { get; set; }
    public bool ReplaceYou { get; set; } = false;
    public bool PrivacyMode { get; set; } = false;

    public bool EnableEncounterHistory { get; set; } = true;
    public int MaxEncounterHistory { get; set; } = 10;
    public bool AutoSwitchToLiveEncounter { get; set; } = true;

    public bool ClearActWithMeter { get; set; } = false;
    public bool ForceEndEncounter { get; set; } = false;

    public Dictionary<uint, Vector4> JobColors { get; set; } = new(JobColorMaps.DefaultColors);

    public Vector4 TankColor { get; set; } = new(0.18f, 0.43f, 0.71f, 1.0f);
    public Vector4 HealerColor { get; set; } = new(0.18f, 0.58f, 0.18f, 1.0f);
    public Vector4 DpsColor { get; set; } = new(0.62f, 0.16f, 0.16f, 1.0f);
    public Vector4 OtherColor { get; set; } = new(0.5f, 0.5f, 0.5f, 1.0f);
}

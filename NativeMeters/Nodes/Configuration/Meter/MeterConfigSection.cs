using System;
using System.Numerics;
using FFXIVClientStructs.FFXIV.Component.GUI;
using KamiToolKit.Nodes;
using NativeMeters.Configuration;

namespace NativeMeters.Nodes.Configuration.Meter;

public abstract class MeterConfigSection : TreeListCategoryNode
{
    protected readonly Func<MeterSettings> GetMeterSettings;
    protected MeterSettings Settings => GetMeterSettings();

    protected MeterConfigSection(Func<MeterSettings> getSettings)
    {
        GetMeterSettings = getSettings;
        VerticalPadding = 4.0f;
    }

    public abstract void Refresh();

    protected static LabelTextNode CreateLabel(string text) => new()
    {
        TextFlags = TextFlags.AutoAdjustNodeSize,
        Size = new Vector2(100, 20),
        String = text,
    };
}
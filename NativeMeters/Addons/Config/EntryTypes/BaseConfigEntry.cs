using System.Reflection;
using KamiToolKit.System;
using NativeMeters.Classes;

namespace NativeMeters.Addons.Config.EntryTypes;

public abstract class BaseConfigEntry : IConfigEntry {
    public required string Label { get; init; }
    public required MemberInfo MemberInfo { get; init; }
    public required ISavable Config { get; init; }

    public abstract NodeBase BuildNode();
}

using KamiToolKit.System;

namespace NativeMeters.Addons.Config.EntryTypes;

public interface IConfigEntry {
    NodeBase BuildNode();
}

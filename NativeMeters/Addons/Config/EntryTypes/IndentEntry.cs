using System;
using KamiToolKit.System;

namespace NativeMeters.Addons.Config.EntryTypes;

public class IndentEntry : IConfigEntry {
    public NodeBase BuildNode()
        => throw new InvalidOperationException();
}

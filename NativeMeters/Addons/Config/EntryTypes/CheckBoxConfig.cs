using KamiToolKit.Nodes;
using KamiToolKit.System;
using NativeMeters.Extensions;

namespace NativeMeters.Addons.Config.EntryTypes;

public class CheckBoxConfig : BaseConfigEntry {
    public required bool InitialState { get; init; }

    public override NodeBase BuildNode() {
        return new CheckboxNode {
            OnClick = OnOptionChanged,
            Height = 24.0f,
            String = Label,
            IsChecked = InitialState,
            IsVisible = true,
        };
    }

    private void OnOptionChanged(bool newValue) {
        MemberInfo.SetValue(Config, newValue);
        Config.Save();
    }
}

using System;
using KamiToolKit.Nodes;

namespace NativeMeters.Nodes.Input;

internal class SyncedCheckboxNode : CheckboxNode
{
    private Action<bool>? onClick;

    public SyncedCheckboxNode()
    {
        base.OnClick = isChecked =>
        {
            SyncVisual(isChecked);
            onClick?.Invoke(isChecked);
        };
    }

    public new bool IsChecked
    {
        get => base.IsChecked;
        set
        {
            base.IsChecked = value;
            SyncVisual(value);
        }
    }

    public override bool IsVisible
    {
        get => base.IsVisible;
        set
        {
            base.IsVisible = value;
            SyncVisual(IsChecked);
        }
    }

    public new Action<bool>? OnClick
    {
        get => onClick;
        set => onClick = value;
    }

    private void SyncVisual(bool isChecked)
        => BoxForeground.IsVisible = IsVisible && isChecked;
}

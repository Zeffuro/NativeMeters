using System;
using System.Numerics;
using KamiToolKit.Nodes;
using NativeMeters.Configuration;
using NativeMeters.Nodes.Input;

namespace NativeMeters.Nodes.Configuration.Meter;

public sealed class MeterGeneralSection : MeterConfigSection
{
    private LabeledTextInputNode? nameInput;
    private CheckboxNode? enabledCheckbox;
    private CheckboxNode? lockedCheckbox;
    private CheckboxNode? clickthroughCheckbox;

    public MeterGeneralSection(Func<MeterSettings> getSettings) : base(getSettings) { }

    public override void Refresh()
    {
        if (nameInput == null) Initialize();

        nameInput!.Text = Settings.Name;
        enabledCheckbox!.IsChecked = Settings.IsEnabled;
        lockedCheckbox!.IsChecked = Settings.IsLocked;
        clickthroughCheckbox!.IsChecked = Settings.IsClickthrough;

        RecalculateLayout();
    }

    private void Initialize()
    {
        AddTab();

        nameInput = new LabeledTextInputNode
        {
            Size = new Vector2(Width, 28),
            LabelText = "Meter Name: ",
            OnInputComplete = val => Settings.Name = val.ToString(),
        };
        AddNode(nameInput);

        enabledCheckbox = new CheckboxNode
        {
            Size = new Vector2(Width, 20),
            String = "Enabled",
            OnClick = val =>
            {
                Settings.IsEnabled = val;
                System.OverlayManager.Setup();
            },
        };
        AddNode(enabledCheckbox);

        lockedCheckbox = new CheckboxNode
        {
            Size = new Vector2(Width, 20),
            String = "Lock Position/Size",
            OnClick = val => Settings.IsLocked = val,
        };
        AddNode(lockedCheckbox);

        clickthroughCheckbox = new CheckboxNode
        {
            Size = new Vector2(Width, 20),
            String = "Clickthrough",
            OnClick = val =>
            {
                Settings.IsClickthrough = val;
                System.OverlayManager.Setup();
            },
        };
        AddNode(clickthroughCheckbox);
    }
}

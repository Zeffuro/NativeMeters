using System;
using System.Numerics;
using KamiToolKit.Nodes;
using NativeMeters.Configuration;
using NativeMeters.Nodes.Input;

namespace NativeMeters.Nodes.Configuration.Meter.Sections;

public sealed class MeterGeneralSection : MeterConfigSection
{
    private LabeledTextInputNode? nameInput;
    private CheckboxRowNode? enabledCheckbox;
    private CheckboxRowNode? lockedCheckbox;
    private CheckboxRowNode? clickthroughCheckbox;
    private bool isLoading;

    public MeterGeneralSection(Func<MeterSettings> getSettings) : base(getSettings) { }

    public override void Refresh()
    {
        if (nameInput == null) Initialize();

        IsInitialized = true;

        isLoading = true;
        try
        {
            nameInput!.Text = Settings.Name;
            enabledCheckbox!.IsChecked = Settings.IsEnabled;
            lockedCheckbox!.IsChecked = Settings.IsLocked;
            clickthroughCheckbox!.IsChecked = Settings.IsClickthrough;
        }
        finally
        {
            isLoading = false;
        }

        RecalculateSectionLayout();
    }

    private void Initialize()
    {
        nameInput = new LabeledTextInputNode
        {
            Size = new Vector2(Width, 28),
            LabelText = "Meter Name: ",
            OnInputComplete = val => Settings.Name = val.ToString(),
        };
        AddNode(nameInput);

        enabledCheckbox = new CheckboxRowNode
        {
            Size = new Vector2(Width, 20),
            String = "Enabled",
            OnClick = val =>
            {
                if (isLoading) return;
                Settings.IsEnabled = val;
                System.OverlayManager.Setup();
            },
        };
        AddNode(enabledCheckbox);

        lockedCheckbox = new CheckboxRowNode
        {
            Size = new Vector2(Width, 20),
            String = "Lock Position/Size",
            OnClick = val =>
            {
                if (isLoading) return;
                Settings.IsLocked = val;
            },
        };
        AddNode(lockedCheckbox);

        clickthroughCheckbox = new CheckboxRowNode
        {
            Size = new Vector2(Width, 20),
            String = "Clickthrough",
            OnClick = val =>
            {
                if (isLoading) return;
                Settings.IsClickthrough = val;
                System.OverlayManager.Setup();
            },
        };
        AddNode(clickthroughCheckbox);
    }
}

using System;
using System.Collections.Generic;
using System.Linq;
using System.Numerics;
using KamiToolKit.Nodes;
using NativeMeters.Configuration;
using NativeMeters.Helpers;
using NativeMeters.Models;
using NativeMeters.Nodes.Input;

namespace NativeMeters.Nodes.Configuration.Meter;

public sealed class MeterDisplaySection : MeterConfigSection
{
    private LabeledDropdownNode? statDropdown;
    private LabeledNumericInputNode? maxRowsInput;
    private CheckboxNode? backgroundCheckbox;
    private CheckboxNode? headerToggle;
    private LabeledNumericInputNode? headerHeightInput;
    private CheckboxNode? footerToggle;
    private LabeledNumericInputNode? footerHeightInput;

    public MeterDisplaySection(Func<MeterSettings> getSettings) : base(getSettings) { }

    public override void Refresh()
    {
        if (statDropdown == null) Initialize();

        statDropdown!.SelectedOption = Settings.StatToTrack;
        maxRowsInput!.Value = Settings.MaxCombatants;
        backgroundCheckbox!.IsChecked = Settings.ShowWindowBackground;
        headerHeightInput!.Value = (int)Settings.HeaderHeight;
        footerHeightInput!.Value = (int)Settings.FooterHeight;
        headerToggle!.IsChecked = Settings.HeaderEnabled;
        footerToggle!.IsChecked = Settings.FooterEnabled;

        RecalculateLayout();
    }

    private void Initialize()
    {
        AddTab();
        statDropdown = new LabeledDropdownNode
        {
            Size = new Vector2(Width, 28),
            LabelText = "Sort By: ",
            Options = CombatantStatHelpers.GetAvailableStatSelectors(),
            OnOptionSelected = val => Settings.StatToTrack = val,
        };

        maxRowsInput = new LabeledNumericInputNode
        {
            Size = new Vector2(Width, 28),
            LabelText = "Max Rows: ",
            Min = 1, Max = 40,
            OnValueUpdate = val => Settings.MaxCombatants = val,
        };

        backgroundCheckbox = new CheckboxNode
        {
            Size = new Vector2(Width, 20),
            String = "Show Background",
            OnClick = val => Settings.ShowWindowBackground = val,
        };

        headerToggle = new CheckboxNode
        {
            Size = new Vector2(Width, 20),
            String = "Enable Header",
            OnClick = val => Settings.HeaderEnabled = val
        };

        headerHeightInput = new LabeledNumericInputNode
        {
            Size = new Vector2(Width, 28),
            LabelText = "Header Space:",
            OnValueUpdate = val => Settings.HeaderHeight = val
        };

        footerToggle = new CheckboxNode
        {
            Size = new Vector2(Width, 20),
            String = "Enable Footer",
            OnClick = val => Settings.FooterEnabled = val
        };

        footerHeightInput = new LabeledNumericInputNode
        {
            Size = new Vector2(Width, 28),
            LabelText = "Footer Space:",
            OnValueUpdate = val => Settings.FooterHeight = val
        };

        AddNode([statDropdown, maxRowsInput, backgroundCheckbox, headerToggle, headerHeightInput, footerToggle, footerHeightInput]);
    }
}

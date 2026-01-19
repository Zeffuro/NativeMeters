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

    public MeterDisplaySection(Func<MeterSettings> getSettings) : base(getSettings) { }

    public override void Refresh()
    {
        if (statDropdown == null) Initialize();

        statDropdown!.SelectedOption = Settings.StatToTrack;
        maxRowsInput!.Value = Settings.MaxCombatants;
        backgroundCheckbox!.IsChecked = Settings.BackgroundEnabled;

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
        AddNode(statDropdown);

        maxRowsInput = new LabeledNumericInputNode
        {
            Size = new Vector2(Width, 28),
            LabelText = "Max Rows: ",
            Min = 1, Max = 40,
            OnValueUpdate = val => Settings.MaxCombatants = val,
        };
        AddNode(maxRowsInput);

        backgroundCheckbox = new CheckboxNode
        {
            Size = new Vector2(Width, 20),
            String = "Show Background",
            OnClick = val => Settings.BackgroundEnabled = val,
        };
        AddNode(backgroundCheckbox);
    }
}

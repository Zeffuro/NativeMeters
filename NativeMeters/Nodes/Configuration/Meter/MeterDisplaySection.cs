using System;
using System.Collections.Generic;
using System.Linq;
using System.Numerics;
using KamiToolKit.Nodes;
using NativeMeters.Configuration;
using NativeMeters.Models;
using NativeMeters.Nodes.Input;

namespace NativeMeters.Nodes.Configuration.Meter;

public sealed class MeterDisplaySection : MeterConfigSection
{
    private LabeledDropdownNode? statDropdown;
    private LabeledDropdownNode? iconTypeDropdown;
    private LabeledDropdownNode? progressBarTypeDropdown;
    private LabeledNumericInputNode? maxRowsInput;
    private CheckboxNode? backgroundCheckbox;

    public MeterDisplaySection(Func<MeterSettings> getSettings) : base(getSettings) { }

    public override void Refresh()
    {
        if (statDropdown == null) Initialize();

        statDropdown!.SelectedOption = Settings.StatToTrack;
        iconTypeDropdown!.SelectedOption = Settings.JobIconType.ToString();
        progressBarTypeDropdown!.SelectedOption = Settings.ProgressBarType.ToString();
        maxRowsInput!.Value = Settings.MaxCombatants;
        backgroundCheckbox!.IsChecked = Settings.BackgroundEnabled;

        RecalculateLayout();
    }

    private void Initialize()
    {
        statDropdown = new LabeledDropdownNode
        {
            Size = new Vector2(Width, 28),
            LabelText = "Stat Type: ",
            Options = new List<string> { "ENCDPS", "ENCHPS", "Damage%", "DPS" },
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

        iconTypeDropdown = new LabeledDropdownNode
        {
            Size = new Vector2(Width, 28),
            LabelText = "Job Icon Type: ",
            Options = Enum.GetNames(typeof(JobIconType)).ToList(),
            OnOptionSelected = selected =>
            {
                if (Enum.TryParse<JobIconType>(selected, out var parsed))
                {
                    Settings.JobIconType = parsed;
                }
            }
        };
        AddNode(iconTypeDropdown);

        progressBarTypeDropdown = new LabeledDropdownNode
        {
            Size = new Vector2(Width, 28),
            LabelText = "Progress Bar Type: ",
            Options = Enum.GetNames(typeof(ProgressBarType)).ToList(),
            OnOptionSelected = selected =>
            {
                if (Enum.TryParse<ProgressBarType>(selected, out var parsed))
                {
                    Settings.ProgressBarType = parsed;
                }
            }
        };
        AddNode(progressBarTypeDropdown);

        backgroundCheckbox = new CheckboxNode
        {
            Size = new Vector2(Width, 20),
            String = "Show Background",
            OnClick = val => Settings.BackgroundEnabled = val,
        };
        AddNode(backgroundCheckbox);
    }
}
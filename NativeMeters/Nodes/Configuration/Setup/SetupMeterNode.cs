using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using KamiToolKit.BaseTypes;
using KamiToolKit.Nodes;
using NativeMeters.Configuration;
using NativeMeters.Configuration.Presets;
using NativeMeters.Data.Stats;
using NativeMeters.Extensions;
using NativeMeters.Models;
using static NativeMeters.Nodes.Configuration.Setup.SetupControls;

namespace NativeMeters.Nodes.Configuration.Setup;

internal sealed class SetupMeterNode : TabbedVerticalListNode
{
    private const float ColumnGap = 12;
    private const float MaxPreviewHeight = 180;
    private const int PreviewDelayMs = 125;
    private const string PresetDefault = "Preset default";
    private static readonly (ProgressBarType Value, string Label)[] BarTypes = ProgressBarTypeOptions.Ordered()
        .Select(type => (type, type.Description)).ToArray();
    private static readonly (ColorMode Value, string Label)[] Colors = EnumChoices<ColorMode>();
    private static readonly (ProgressBarColorTreatment Value, string Label)[] Tints = EnumChoices<ProgressBarColorTreatment>();
    private static readonly (ProgressBarSizing Value, string Label)[] BarSizes = EnumChoices<ProgressBarSizing>();
    private static readonly (MeterLayout Value, string Label)[] Layouts = EnumChoices<MeterLayout>();
    private static readonly (JobIconType Value, string Label)[] IconStyles = EnumChoices<JobIconType>();
    private static readonly (bool Value, string Label)[] TextPositions = [(true, "Above bar"), (false, "Inside bar")];
    private static readonly (string Value, string Label)[] Stats = StatSelector.GetAvailableStatSelectors()
        .Select(stat => (stat, StatLabel(stat))).ToArray();

    private readonly CustomMeterOptions custom = new() { ShowBackground = true };
    private readonly TabbedVerticalListNode form;
    private readonly TabbedVerticalListNode presetControls;
    private TabbedVerticalListNode? customControls;
    private TabbedVerticalListNode? appearance;
    private TabbedVerticalListNode? columns;
    private TabBarNode? customTabs;
    private readonly TabBarNode tabs;
    private readonly List<SetupChoiceNode> choices = [];
    private readonly SetupChoiceNode[] presetBarControls;
    private readonly SetupChoiceNode presetIconChoice;
    private SetupChoiceNode? customIconChoice;
    private readonly SetupChoiceNode presetBarSizingChoice;
    private SetupChoiceNode? customBarSizingChoice;
    private readonly SetupMeterPreviewNode preview;
    private readonly TextNode previewLabel;
    private readonly Action changed;
    private readonly float contentHeight;
    private string preset;
    private bool useCustom;
    private bool showColumns;
    private bool previewDirty = true;
    private long previewRequestedAt;
    private CustomMeterBarStyle? presetBar;
    private ProgressBarType? presetBarType;
    private bool? presetBackground;
    private ColorMode? presetPlayerColor;
    private ColorMode? presetBarColor;
    private ProgressBarColorTreatment? presetBarTint;
    private string? presetSort;
    private string? presetFill;
    private JobIconType? presetIconStyle;
    private ProgressBarSizing? presetBarSizing;

    public bool CanContinue => !useCustom || custom.Columns.Count != 0;

    public SetupMeterNode(float width, float contentHeight, SetupSampleFight sample, Action changed)
    {
        this.changed = changed;
        this.contentHeight = contentHeight;
        Width = width;
        FitWidth = true;
        ItemSpacing = Spacing;
        TabSize = Indent;

        var presets = MeterPresets.GetPresetNames();
        preset = presets[0];
        var controlsWidth = width - TabSize - ItemSpacing;
        tabs = new TabBarNode { Width = controlsWidth, Height = 26 };
        tabs.AddTab("Use a preset", () => SetMode(false));
        tabs.AddTab("Build your own", () => SetMode(true));

        var presetChoice = Choice("Preset", presets.Select(name => (name, name)).ToArray(), preset, value => preset = value);
        var presetType = Override("Bar type", BarTypes, value => presetBarType = value);
        var presetTint = Override("Bar tint", Tints, value => presetBarTint = value);
        var presetColor = Override("Player color", Colors, value => presetPlayerColor = value);
        presetColor.SetTooltip("Color player names by job, role, or a fixed color.");
        var barColor = Override("Bar color", Colors, value => presetBarColor = value);
        var fill = StatOverride("Bar fill", value => presetFill = value);
        presetIconChoice = Override("Icon Style", IconStyles, value => presetIconStyle = value);
        presetBarSizingChoice = Override("Bar sizing", BarSizes, value => presetBarSizing = value);
        presetBarControls = [presetType, presetTint, barColor, fill];
        presetControls = Page(controlsWidth);
        presetControls.AddNode([
            presetChoice,
            Pair(controlsWidth, Override("Bar style", CustomMeterPresets.BarStyles, value => presetBar = value), presetType),
            Pair(controlsWidth, presetColor, barColor),
            Pair(controlsWidth, presetTint, fill),
            Pair(controlsWidth, StatOverride("Sort by", value => presetSort = value), presetBarSizingChoice),
            Pair(controlsWidth, Override<bool>("Background", [(true, "On"), (false, "Off")], value => presetBackground = value),
                presetIconChoice),
        ]);
        presetControls.RecalculateLayout();

        form = Page(controlsWidth);
        form.FitWidth = false;
        form.AddNode(presetControls);
        previewLabel = Section("Sample fight");
        preview = new SetupMeterPreviewNode(sample) { Height = MaxPreviewHeight };
        AddNode(1, [tabs, form, previewLabel, preview]);
        RecalculateLayout();
    }

    private TabbedVerticalListNode BuildCustomControls(float width)
    {
        var customType = Choice("Bar type", BarTypes, custom.BarType ?? ProgressBarType.Cast, value => custom.BarType = value);
        var customTint = Choice("Bar tint", Tints, custom.BarTint ?? ProgressBarColorTreatment.Auto, value => custom.BarTint = value);
        var customColor = Override("Player color", Colors, value => custom.PlayerColor = value, "Default");
        customColor.SetTooltip("Color player names by job, role, or a fixed color.");
        var barColor = Override("Bar color", Colors, value => custom.BarColor = value, "Default");
        var fill = Choice("Bar fill", Stats, custom.FillStat, value => custom.FillStat = value);
        var position = Choice("Text position", TextPositions, custom.TextAboveBar, value => custom.TextAboveBar = value);
        customIconChoice = Choice("Icon Style", IconStyles, custom.IconStyle, value => custom.IconStyle = value);
        customBarSizingChoice = Choice("Bar sizing", BarSizes, custom.BarSizing, value => custom.BarSizing = value);
        var footer = Check("Footer", custom.ShowFooter, value =>
        {
            custom.ShowFooter = value;
            QueuePreview();
        });
        footer.TextTooltip = "Show deaths and party DPS below the meter.";
        var background = Check("Background", true, value =>
        {
            custom.ShowBackground = value;
            QueuePreview();
        });
        background.Width = footer.Width = MathF.Floor((PairFieldWidth(width) - ColumnGap) / 2);
        var extras = new HorizontalListNode { Height = CheckHeight, ItemSpacing = ColumnGap, InitialNodes = [background, footer] };
        appearance = Page(width);
        appearance.AddNode([
            Pair(width, Choice("Player layout", Layouts, custom.Layout, value => custom.Layout = value), position),
            Pair(width, Choice("Bar style", CustomMeterPresets.BarStyles, custom.BarStyle, value =>
            {
                custom.BarStyle = value;
                customType.IsEnabled = customTint.IsEnabled = barColor.IsEnabled = fill.IsEnabled = value != CustomMeterBarStyle.None;
                position.IsEnabled = value == CustomMeterBarStyle.FullWidth;
                changed();
            }), customType),
            Pair(width, customColor, barColor),
            Pair(width, customTint, fill),
            Pair(width, Choice("Sort by", Stats, custom.SortBy, value => custom.SortBy = value), customBarSizingChoice),
            Pair(width, extras, customIconChoice),
        ]);
        columns = Page(width);
        AddColumns(columns);
        columns.IsVisible = false;
        customTabs = new TabBarNode { Width = width, Height = 26 };
        customTabs.AddTab("Appearance", () => ShowCustomSection(false));
        customTabs.AddTab("Columns", () => ShowCustomSection(true));
        var controls = Page(width);
        controls.FitWidth = false;
        controls.AddNode([customTabs, appearance, columns]);
        return controls;
    }

    private void ShowCustomSection(bool displayColumns)
    {
        if (showColumns == displayColumns || appearance == null || columns == null) return;
        CollapseChoices();
        showColumns = displayColumns;
        appearance.IsVisible = !displayColumns;
        columns.IsVisible = displayColumns;
        Refresh();
    }

    private void AddColumns(TabbedVerticalListNode controls)
    {
        const int columnsPerRow = 3;
        var columnWidth = MathF.Floor((controls.Width - Spacing * columnsPerRow) / columnsPerRow);
        var rows = CustomMeterPresets.Columns.Chunk(columnsPerRow).Select(columns => new HorizontalListNode
        {
            Height = CheckHeight,
            ItemSpacing = Spacing,
            InitialNodes = columns.Select(entry =>
            {
                var checkbox = Check(entry.Label, custom.Columns.Contains(entry.Column), enabled => SetColumn(entry.Column, enabled));
                checkbox.Width = columnWidth;
                return (NodeBase)checkbox;
            }).ToArray(),
        }).ToArray();
        controls.AddNode(rows);
    }

    private void SetColumn(CustomMeterColumn column, bool enabled)
    {
        var couldContinue = CanContinue;
        if (enabled)
        {
            var index = custom.Columns.BinarySearch(column);
            if (index < 0) custom.Columns.Insert(~index, column);
        }
        else
        {
            custom.Columns.Remove(column);
        }

        if (couldContinue != CanContinue) Refresh();
        else QueuePreview();
    }

    private static (T Value, string Label)[] EnumChoices<T>() where T : struct, Enum
        => Enum.GetValues<T>().Select(value => (value, value.Description)).ToArray();

    private static string StatLabel(string stat) => stat switch
    {
        "ENCDPS" => "Encounter DPS",
        "ENCHPS" => "Encounter HPS",
        "DPS" => "Active DPS",
        "Damage%" => "Damage share",
        "DirectHit%" => "Direct hit %",
        "CritHit%" => "Crit. hit %",
        _ => stat,
    };

    private SetupChoiceNode StatOverride(string title, Action<string?> apply)
        => AddChoice(title, Stats.Select(entry => entry.Label).Prepend(PresetDefault).ToArray(), 0,
            index => apply(index == 0 ? null : Stats[index - 1].Value));

    private SetupChoiceNode Choice<T>(string title, IReadOnlyList<(T Value, string Label)> entries, T current, Action<T> apply)
    {
        var values = entries.Select(entry => entry.Value).ToArray();
        return AddChoice(title, entries.Select(entry => entry.Label).ToArray(), Array.IndexOf(values, current), i => apply(values[i]));
    }

    private SetupChoiceNode Override<T>(string title, IReadOnlyList<(T Value, string Label)> entries, Action<T?> apply,
        string defaultLabel = PresetDefault) where T : struct
        => AddChoice(title, entries.Select(entry => entry.Label).Prepend(defaultLabel).ToArray(), 0,
            index => apply(index == 0 ? null : entries[index - 1].Value));

    private SetupChoiceNode AddChoice(string title, IReadOnlyList<string> labels, int selected, Action<int> apply)
    {
        var choice = new SetupChoiceNode(title, labels, Math.Max(0, selected), index =>
        {
            apply(index);
            QueuePreview();
        });
        choices.Add(choice);
        return choice;
    }

    private static float PairFieldWidth(float width) => MathF.Floor((width - Spacing - ColumnGap) / 2);

    private static HorizontalListNode Pair(float width, NodeBase first, NodeBase? second = null)
    {
        var fieldWidth = PairFieldWidth(width);
        first.Width = fieldWidth;
        if (second != null) second.Width = fieldWidth;
        return new HorizontalListNode
        {
            Height = Math.Max(first.Height, second?.Height ?? 0),
            ItemSpacing = ColumnGap,
            InitialNodes = second == null ? [first] : [first, second],
        };
    }

    public void CollapseChoices()
    {
        foreach (var choice in choices) choice.Collapse();
    }

    public int ApplyNavigation(int start, int footer)
    {
        const int tabCount = 2;
        var firstField = start + tabCount;
        var selectedTab = start + (useCustom ? 1 : 0);
        int end;
        if (useCustom && customTabs != null && appearance != null && columns != null)
        {
            var firstChoice = firstField + tabCount;
            var selectedSection = firstField + (showColumns ? 1 : 0);
            end = ConfigurationNavigation.Apply(showColumns ? columns : appearance, firstChoice, selectedSection, footer);
            customTabs.NavIndex = firstField;
            customTabs.NavUp = selectedTab;
            customTabs.NavDown = end == firstChoice ? footer : firstChoice;
            customTabs.Width = form.Width;
        }
        else
        {
            end = ConfigurationNavigation.Apply(presetControls, firstField, selectedTab, footer);
        }
        tabs.NavIndex = start;
        tabs.NavUp = footer;
        tabs.NavDown = end == firstField ? footer : firstField;
        tabs.Width = form.Width;
        return end;
    }

    private void SetMode(bool customMode)
    {
        if (useCustom == customMode) return;
        CollapseChoices();
        useCustom = customMode;
        presetControls.IsVisible = !useCustom;
        if (useCustom && customControls == null)
        {
            customControls = BuildCustomControls(form.Width);
            form.AddNode(customControls);
        }
        if (customControls != null) customControls.IsVisible = useCustom;
        form.RecalculateLayout();
        Refresh();
    }

    private void Refresh()
    {
        previewLabel.String = CanContinue ? "Sample fight" : "Pick at least one column.";
        preview.IsVisible = CanContinue;
        RecalculateLayout();
        QueuePreview();
        changed();
    }

    private void QueuePreview()
    {
        previewDirty = true;
        previewRequestedAt = Stopwatch.GetTimestamp();
    }

    public void UpdatePreview()
    {
        if (!previewDirty || !CanContinue) return;
        if (previewRequestedAt != 0 && Stopwatch.GetElapsedTime(previewRequestedAt).TotalMilliseconds < PreviewDelayMs) return;

        previewDirty = false;
        preview.Height = Math.Clamp(contentHeight - preview.Y, 0, MaxPreviewHeight);
        var meter = CreateMeter();
        var navigationChanged = false;
        var bar = meter.RowComponents.Find(component => component?.Type == MeterComponentType.ProgressBar);
        if (!useCustom)
        {
            var hasBar = bar != null;
            if (presetBarControls[0].IsEnabled != hasBar)
            {
                foreach (var choice in presetBarControls) choice.IsEnabled = hasBar;
                navigationChanged = true;
            }
        }

        var sizingChoice = useCustom ? customBarSizingChoice! : presetBarSizingChoice;
        var canPreserveEnds = bar != null && ProgressBarTypeOptions.SupportsPreservingEnds(bar.ProgressBarType);
        if (sizingChoice.IsEnabled != canPreserveEnds)
        {
            sizingChoice.IsEnabled = canPreserveEnds;
            navigationChanged = true;
        }

        var iconChoice = useCustom ? customIconChoice! : presetIconChoice;
        var hasIcon = meter.RowComponents.Exists(component => component?.Type == MeterComponentType.JobIcon);
        if (iconChoice.IsEnabled != hasIcon)
        {
            iconChoice.IsEnabled = hasIcon;
            navigationChanged = true;
        }

        if (navigationChanged) changed();

        var rowCount = useCustom && meter.Layout == MeterLayout.Vertical ? 2 : 3;
        preview.ShowMeter(meter, rowCount);
    }

    public MeterSettings CreateMeter(string name = "DPS")
    {
        var meter = new MeterSettings { Name = name };
        if (useCustom)
            CustomMeterPresets.Apply(custom, meter);
        else
            MeterPresets.ApplyPreset(preset, meter, presetBar, presetBackground, presetBarType, presetPlayerColor, presetBarTint,
                presetBarColor, presetSort, presetFill, presetIconStyle, presetBarSizing);
        return meter;
    }
}

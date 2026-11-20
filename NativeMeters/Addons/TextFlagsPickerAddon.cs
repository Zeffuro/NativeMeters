using System;
using System.Collections.Generic;
using System.Numerics;
using FFXIVClientStructs.FFXIV.Component.GUI;
using KamiToolKit.BaseTypes;
using KamiToolKit.Enums;
using KamiToolKit.Nodes;
using NativeMeters.Nodes.Input;
using NativeMeters.Localization;

namespace NativeMeters.Addons;

public sealed class TextFlagsPickerAddon : NativeAddon
{
    private const float RowHeight = 20.0f;
    private const float ButtonHeight = 28.0f;
    private const float ButtonWidth = 92.0f;
    private const float RowSpacing = 2.0f;
    private const float ButtonSpacing = 8.0f;
    private const float HelpButtonSize = 22.0f;
    private readonly List<TextFlagCheckboxEntry> flagEntries = [];

    private TextFlags value;
    private Action<TextFlags>? onValueChanged;
    private bool isLoading;

    public void OpenFor(TextFlags initialValue, Action<TextFlags> onValueChanged)
    {
        value = initialValue;
        this.onValueChanged = onValueChanged;

        if (IsOpen)
        {
            RefreshCheckboxes();
            return;
        }

        Open();
    }

    protected override unsafe void OnSetup(AtkUnitBase* addon, Span<AtkValue> atkValueSpan)
    {
        base.OnSetup(addon, atkValueSpan);
        this.SetLocalizedTitle("WindowTextFlags");

        AddNode(new CircleButtonNode
        {
            Icon = CircleButtonIcon.QuestionMark,
            Size = new Vector2(HelpButtonSize),
            X = (int)Math.Round(Size.X - HelpButtonSize * 2.5),
            Y = HelpButtonSize / 2,
            TextTooltip = Loc.Get("PickerTextFlagsHelp"),
        });

        flagEntries.Clear();

        var layoutNode = new VerticalListNode
        {
            Position = ContentStartPosition,
            Size = ContentSize,
            FitContents = true,
            FitWidth = true,
            ItemSpacing = RowSpacing,
        };

        foreach (var flag in TextFlagOptions.SelectableFlags)
        {
            var checkbox = new CheckboxRowNode
            {
                Height = RowHeight,
                String = TextFlagOptions.GetLabel(flag),
                OnClick = isChecked => SetFlag(flag, isChecked),
            };

            layoutNode.AddNode(checkbox);
            flagEntries.Add(new TextFlagCheckboxEntry(flag, checkbox));
        }

        layoutNode.AddNode(new ResNode { Height = ButtonSpacing });
        layoutNode.AddNode(CreateButtonRow());

        var windowPadding = Size.Y - ContentSize.Y;
        SetWindowSize(Size.X, windowPadding + layoutNode.Height);
        layoutNode.AttachNode(this);

        RefreshCheckboxes();
        addon->UldManager.SetupTextRecursive();
    }

    protected override unsafe void OnFinalize(AtkUnitBase* addon)
    {
        flagEntries.Clear();
        onValueChanged = null;

        base.OnFinalize(addon);
    }

    private HorizontalListNode CreateButtonRow()
        => new()
        {
            Height = ButtonHeight,
            FitHeight = true,
            ItemSpacing = ButtonSpacing,
            InitialNodes =
            [
                new TextButtonNode
                {
                    Size = new Vector2(ButtonWidth, ButtonHeight),
                    String = Loc.Get("PickerClear"),
                    OnClick = Clear,
                    NavIndex = 20,
                    NavRight = 21,
                },
                new TextButtonNode
                {
                    Size = new Vector2(ButtonWidth, ButtonHeight),
                    String = Loc.Get("CommonClose"),
                    OnClick = Close,
                    NavIndex = 21,
                    NavLeft = 20,
                },
            ],
        };

    private void SetFlag(TextFlags flag, bool isChecked)
    {
        if (isLoading)
            return;

        value = isChecked
            ? value | flag
            : value & ~flag;

        RefreshCheckboxes();
        onValueChanged?.Invoke(value);
    }

    private void Clear()
    {
        value = default;
        RefreshCheckboxes();
        onValueChanged?.Invoke(value);
    }

    private void RefreshCheckboxes()
    {
        isLoading = true;

        try
        {
            foreach (var entry in flagEntries)
            {
                entry.Checkbox.IsChecked = value.HasFlag(entry.Flag);
            }
        }
        finally
        {
            isLoading = false;
        }
    }

    private sealed record TextFlagCheckboxEntry(TextFlags Flag, CheckboxRowNode Checkbox);
}

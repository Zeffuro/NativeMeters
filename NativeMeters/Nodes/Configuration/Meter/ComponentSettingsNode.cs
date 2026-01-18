using System;
using System.Collections.Generic;
using System.Linq;
using System.Numerics;
using FFXIVClientStructs.FFXIV.Component.GUI;
using KamiToolKit.Nodes;
using NativeMeters.Configuration;
using NativeMeters.Nodes.Color;
using NativeMeters.Nodes.Input;
using NativeMeters.Nodes.LayoutNodes;

namespace NativeMeters.Nodes.Configuration.Meter;

public sealed class ComponentSettingsNode : FakeCategoryNode
{
    private RowComponentSettings? settings;

    private readonly LabeledTextInputNode nameInput;
    private readonly LabeledTextInputNode sourceInput;
    private readonly LabeledNumericInputNode posXInput;
    private readonly LabeledNumericInputNode posYInput;
    private readonly LabeledNumericInputNode widthInput;
    private readonly LabeledNumericInputNode heightInput;
    private readonly LabeledNumericInputNode zIndexInput;

    private readonly LabeledDropdownNode fontTypeDropdown;
    private readonly LabeledDropdownNode textFlagsDropdown;
    private readonly LabeledNumericInputNode fontSizeInput;

    private readonly ColorInputRow textColorInput;
    private readonly CheckboxNode jobColorCheckbox;
    private readonly CheckboxNode outlineCheckbox;
    private readonly ColorInputRow outlineColorInput;

    private readonly LabelTextNode typographyLabel;
    private readonly LabelTextNode visualLabel;
    private readonly HorizontalListNode tagHelper;
    private readonly TextButtonNode deleteBtn;

    public Action? OnChanged { get; set; }
    public Action? OnDeleted { get; set; }

    public RowComponentSettings? RowComponent
    {
        get => settings;
        set
        {
            settings = value;
            if (settings == null) return;

            String = $"{settings.Type} - {settings.Name}";

            nameInput.Text = settings.Name;
            sourceInput.Text = settings.DataSource;
            posXInput.Value = (int)settings.Position.X;
            posYInput.Value = (int)settings.Position.Y;
            widthInput.Value = (int)settings.Size.X;
            heightInput.Value = (int)settings.Size.Y;
            zIndexInput.Value = settings.ZIndex;

            fontTypeDropdown.SelectedOption = settings.FontType.ToString();
            textFlagsDropdown.SelectedOption = settings.TextFlags.ToString();
            fontSizeInput.Value = (int)settings.FontSize;

            var isText = settings.Type == MeterComponentType.Text;

            typographyLabel.IsVisible = isText;
            fontTypeDropdown.IsVisible = isText;
            textFlagsDropdown.IsVisible = isText;
            fontSizeInput.IsVisible = isText;
            outlineCheckbox.IsVisible = isText;
            outlineColorInput.IsVisible = isText && settings.ShowOutline;

            jobColorCheckbox.IsVisible = isText || settings.Type == MeterComponentType.ProgressBar;
            textColorInput.IsVisible = isText || settings.Type == MeterComponentType.Background;
            visualLabel.IsVisible = jobColorCheckbox.IsVisible || textColorInput.IsVisible;

            textColorInput.CurrentColor = settings.Color;
            outlineColorInput.CurrentColor = settings.OutlineColor;
            jobColorCheckbox.IsChecked = settings.UseJobColor;
            outlineCheckbox.IsChecked = settings.ShowOutline;

            Content.RecalculateLayout();
            RecalculateLayout();
        }
    }

    public ComponentSettingsNode()
    {
        Content.ItemSpacing = 4.0f;
        Content.FirstItemSpacing = 6.0f;

        nameInput = new LabeledTextInputNode
        {
            LabelText = "Display Name: ",
            Size = new Vector2(Width, 28),
            OnInputComplete = val =>
            {
                if (settings == null) return;
                settings.Name = val.ToString();
                String = $"{settings.Type} - {val}";
                OnChanged?.Invoke();
            }
        };

        sourceInput = new LabeledTextInputNode
        {
            LabelText = "Format/Tags: ",
            Size = new Vector2(Width, 28),
            Placeholder = "e.g. [name] ([dps])",
            OnInputComplete = val => {
                settings?.DataSource = val.ToString();
                OnChanged?.Invoke(); }
        };

        tagHelper = new HorizontalListNode
        {
            Size = new Vector2(Width, 24),
            ItemSpacing = 5.0f
        };

        tagHelper.AddNode(new LabelTextNode { String = "Tags: ", Size = new Vector2(40, 20) });

        string[] tags = ["[name]", "[dps]", "[dps:k1]", "[damage%]", "[hps]"];
        foreach (var tag in tags)
        {
            tagHelper.AddNode(new TextButtonNode
            {
                String = tag,
                Size = new Vector2(55, 20),
                OnClick = () =>
                {
                    sourceInput.Text += tag;
                    sourceInput.InnerInput.OnInputComplete?.Invoke(sourceInput.Text);
                }
            });
        }

        var posRow = new HorizontalListNode { Size = new Vector2(Width, 30), ItemSpacing = 8.0f };
        posXInput = new LabeledNumericInputNode { LabelText = "X:", Size = new Vector2(176, 28), OnValueUpdate = v => {
            settings?.Position = settings.Position with {X=v};
            OnChanged?.Invoke(); }};
        posYInput = new LabeledNumericInputNode { LabelText = "Y:", Size = new Vector2(176, 28), OnValueUpdate = v => {
            settings?.Position = settings.Position with {Y=v};
            OnChanged?.Invoke(); }};
        posRow.AddNode([posXInput, posYInput]);

        var sizeRow = new HorizontalListNode { Size = new Vector2(Width, 30), ItemSpacing = 8.0f };
        widthInput = new LabeledNumericInputNode { LabelText = "W:", Size = new Vector2(176, 28), OnValueUpdate = v => {
            settings?.Size = settings.Size with {X=v};
            OnChanged?.Invoke(); }};
        heightInput = new LabeledNumericInputNode { LabelText = "H:", Size = new Vector2(176, 28), OnValueUpdate = v => {
            settings?.Size = settings.Size with {Y=v};
            OnChanged?.Invoke(); }};
        sizeRow.AddNode([widthInput, heightInput]);

        typographyLabel = new LabelTextNode
        {
            String = "Typography",
            Size = new Vector2(Width, 20),
            TextColor = new Vector4(0.7f, 0.7f, 1f, 1f)
        };

        fontTypeDropdown = new LabeledDropdownNode
        {
            LabelText = "Font:",
            Size = new Vector2(Width, 28),
            Options = Enum.GetNames<FontType>().ToList(),
            OnOptionSelected = val =>
            {
                if(Enum.TryParse<FontType>(val, out var f)) settings!.FontType = f; OnChanged?.Invoke();
            }
        };

        textFlagsDropdown = new LabeledDropdownNode
        {
            LabelText = "Style:",
            Size = new Vector2(Width, 28),
            Options = Enum.GetNames<TextFlags>().ToList(),
            OnOptionSelected = val =>
            {
                if(Enum.TryParse<TextFlags>(val, out var f)) settings!.TextFlags = f; OnChanged?.Invoke();
            }
        };

        fontSizeInput = new LabeledNumericInputNode { LabelText = "Size:", Size = new Vector2(Width, 28), Min = 6, Max = 72, OnValueUpdate = v => {
            settings?.FontSize = (uint)v;
            OnChanged?.Invoke(); }};

        zIndexInput = new LabeledNumericInputNode { LabelText = "Z-Order:", Size = new Vector2(Width, 28), OnValueUpdate = v => {
            settings?.ZIndex = v;
            OnChanged?.Invoke(); }};

        visualLabel = new LabelTextNode
        {
            String = "Visuals",
            Size = new Vector2(Width, 20),
            TextColor = new Vector4(0.7f, 0.7f, 1f, 1f)
        };

        jobColorCheckbox = new CheckboxNode { String = "Use Job Color", Size = new Vector2(Width, 22), OnClick = v => {
            settings?.UseJobColor = v;
            OnChanged?.Invoke(); }};

        textColorInput = new ColorInputRow
        {
            Label = "Static Color",
            Size = new Vector2(Width, 28),
            DefaultColor = Vector4.One,
            OnColorConfirmed = c => { if (settings != null) settings.Color = c; OnChanged?.Invoke(); },
            CurrentColor = Vector4.One
        };

        outlineCheckbox = new CheckboxNode
        {
            String = "Show Outline",
            Size = new Vector2(Width, 22),
            OnClick = v =>
            {
                if (settings != null) settings.ShowOutline = v;
                outlineColorInput.IsVisible = v;
                Content.RecalculateLayout();
                OnChanged?.Invoke();
            }
        };

        outlineColorInput = new ColorInputRow
        {
            Label = "Outline Color",
            Size = new Vector2(Width, 28),
            DefaultColor = new Vector4(0, 0, 0, 1),
            OnColorConfirmed = c => { if (settings != null) settings.OutlineColor = c; OnChanged?.Invoke(); },
            CurrentColor = new Vector4(0, 0, 0, 1)
        };

        deleteBtn = new TextButtonNode
        {
            String = "Delete Component",
            Size = new Vector2(Width, 24),
            Color = KamiToolKit.Classes.ColorHelper.GetColor(17),
            OnClick = () => OnDeleted?.Invoke()
        };

        Content.AddNode([
            nameInput, sourceInput, tagHelper, posRow, sizeRow,
            typographyLabel, fontTypeDropdown, fontSizeInput, textFlagsDropdown, zIndexInput,
            visualLabel, jobColorCheckbox, textColorInput, outlineCheckbox, outlineColorInput,
            deleteBtn
        ]);
    }

    protected override void OnSizeChanged()
    {
        base.OnSizeChanged();
        if (nameInput == null) return;

        var innerWidth = Content.Width - 10.0f;

        nameInput.Width = innerWidth;
        sourceInput.Width = innerWidth;
        fontTypeDropdown.Width = innerWidth;
        textFlagsDropdown.Width = innerWidth;
        fontSizeInput.Width = innerWidth;
        zIndexInput.Width = innerWidth;
        textColorInput.Width = innerWidth;
        outlineColorInput.Width = innerWidth;
        tagHelper.Width = innerWidth;
        typographyLabel.Width = innerWidth;
        visualLabel.Width = innerWidth;
        deleteBtn.Width = innerWidth;
    }
}
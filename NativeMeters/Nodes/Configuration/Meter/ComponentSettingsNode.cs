using System;
using System.Collections.Generic;
using System.Linq;
using System.Numerics;
using FFXIVClientStructs.FFXIV.Component.GUI;
using KamiToolKit.Classes;
using KamiToolKit.Nodes;
using NativeMeters.Configuration;
using NativeMeters.Nodes.Color;
using NativeMeters.Nodes.Input;
using NativeMeters.Nodes.LayoutNodes;

namespace NativeMeters.Nodes.Configuration.Meter;

public sealed class ComponentSettingsNode : FakeCategoryNode
{
    private ComponentSettings? settings;

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
    private readonly CheckboxNode backgroundCheckbox;

    private readonly LabelTextNode typographyLabel;
    private readonly LabelTextNode visualLabel;
    private readonly HorizontalListNode tagHelper;
    private readonly TextButtonNode deleteBtn;

    public Action? OnChanged { get; set; }
    public Action? OnDeleted { get; set; }

    public ComponentSettings? RowComponent
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
            backgroundCheckbox.IsVisible = isText;
            visualLabel.IsVisible = true;

            textColorInput.CurrentColor = settings.TextColor;
            outlineColorInput.CurrentColor = settings.TextOutlineColor;
            jobColorCheckbox.IsChecked = settings.UseJobColor;
            outlineCheckbox.IsChecked = settings.ShowOutline;
            backgroundCheckbox.IsChecked = settings.ShowBackground;

            Content.RecalculateLayout();
            RecalculateLayout();
        }
    }

    public ComponentSettingsNode()
    {
        Content.ItemSpacing = 4.0f;
        Content.FirstItemSpacing = 6.0f;
        HeaderHeight = 24.0f;
        NestingIndent = 12.0f;

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
            LabelText = "Format String: ",
            Size = new Vector2(Width, 28),
            Placeholder = "[name] [dps]",
            OnInputComplete = val =>
            {
                if (settings != null) settings.DataSource = val.ToString();
                OnChanged?.Invoke();
            }
        };

        tagHelper = new HorizontalListNode
        {
            Size = new Vector2(Width, 24),
            ItemSpacing = 5.0f
        };

        tagHelper.AddNode(new LabelTextNode
        {
            String = "Tags: ",
            Size = new Vector2(40, 20)
        });

        foreach (var tag in new[] { "[name]", "[dps]", "[dps:k1]", "[damage%]", "[hps]" })
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

        var posRow = new HorizontalListNode
        {
            Size = new Vector2(Width, 30),
            ItemSpacing = 8.0f
        };

        posXInput = new LabeledNumericInputNode
        {
            LabelText = "Pos X:",
            Size = new Vector2(176, 28),
            OnValueUpdate = val =>
            {
                if (settings != null) settings.Position = settings.Position with { X = val };
                OnChanged?.Invoke();
            }
        };

        posYInput = new LabeledNumericInputNode
        {
            LabelText = "Pos Y:",
            Size = new Vector2(176, 28),
            OnValueUpdate = val =>
            {
                if (settings != null) settings.Position = settings.Position with { Y = val };
                OnChanged?.Invoke();
            }
        };
        posRow.AddNode([posXInput, posYInput]);

        var sizeRow = new HorizontalListNode
        {
            Size = new Vector2(Width, 30),
            ItemSpacing = 8.0f
        };

        widthInput = new LabeledNumericInputNode
        {
            LabelText = "Width:",
            Size = new Vector2(176, 28),
            OnValueUpdate = val =>
            {
                if (settings != null) settings.Size = settings.Size with { X = val };
                OnChanged?.Invoke();
            }
        };

        heightInput = new LabeledNumericInputNode
        {
            LabelText = "Height:",
            Size = new Vector2(176, 28),
            OnValueUpdate = val =>
            {
                if (settings != null) settings.Size = settings.Size with { Y = val };
                OnChanged?.Invoke();
            }
        };
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
                if (Enum.TryParse<FontType>(val, out var font)) settings!.FontType = font;
                OnChanged?.Invoke();
            }
        };

        textFlagsDropdown = new LabeledDropdownNode
        {
            LabelText = "Style:",
            Size = new Vector2(Width, 28),
            Options = Enum.GetNames<TextFlags>().ToList(),
            OnOptionSelected = val =>
            {
                if (Enum.TryParse<TextFlags>(val, out var flags)) settings!.TextFlags = flags;
                OnChanged?.Invoke();
            }
        };

        fontSizeInput = new LabeledNumericInputNode
        {
            LabelText = "Size:",
            Size = new Vector2(Width, 28),
            Min = 6,
            Max = 72,
            OnValueUpdate = val =>
            {
                if (settings != null) settings.FontSize = (uint)val;
                OnChanged?.Invoke();
            }
        };

        zIndexInput = new LabeledNumericInputNode
        {
            LabelText = "Z-Order:",
            Size = new Vector2(Width, 28),
            OnValueUpdate = val =>
            {
                if (settings != null) settings.ZIndex = val;
                OnChanged?.Invoke();
            }
        };

        visualLabel = new LabelTextNode
        {
            String = "Visuals",
            Size = new Vector2(Width, 20),
            TextColor = new Vector4(0.7f, 0.7f, 1f, 1f)
        };

        jobColorCheckbox = new CheckboxNode
        {
            String = "Use Job Color",
            Size = new Vector2(Width, 22),
            OnClick = val =>
            {
                if (settings != null) settings.UseJobColor = val;
                OnChanged?.Invoke();
            }
        };

        textColorInput = new ColorInputRow
        {
            Label = "Static Color",
            Size = new Vector2(Width, 28),
            DefaultColor = Vector4.One,
            OnColorConfirmed = color =>
            {
                if (settings != null) settings.TextColor = color;
                OnChanged?.Invoke();
            },
            CurrentColor = Vector4.One
        };

        outlineCheckbox = new CheckboxNode
        {
            String = "Show Outline",
            Size = new Vector2(Width, 22),
            OnClick = val =>
            {
                if (settings != null) settings.ShowOutline = val;
                outlineColorInput.IsVisible = val;
                Content.RecalculateLayout();
                OnChanged?.Invoke();
            }
        };

        outlineColorInput = new ColorInputRow
        {
            Label = "Outline Color",
            Size = new Vector2(Width, 28),
            DefaultColor = new Vector4(0, 0, 0, 1),
            OnColorConfirmed = color =>
            {
                if (settings != null) settings.TextOutlineColor = color;
                OnChanged?.Invoke();
            },
            CurrentColor = new Vector4(0, 0, 0, 1)
        };

        backgroundCheckbox = new CheckboxNode
        {
            String = "Show BG",
            Size = new Vector2(Width, 22),
            OnClick = val =>
            {
                if (settings != null) settings.ShowBackground = val;
                OnChanged?.Invoke();
            }
        };

        deleteBtn = new TextButtonNode
        {
            String = "Delete Component",
            Size = new Vector2(Width, 24),
            Color = ColorHelper.GetColor(17),
            OnClick = () => OnDeleted?.Invoke()
        };

        Content.AddNode([
            nameInput, sourceInput, tagHelper, posRow, sizeRow,
            typographyLabel, fontTypeDropdown, fontSizeInput, textFlagsDropdown, zIndexInput,
            visualLabel, jobColorCheckbox, textColorInput, outlineCheckbox, outlineColorInput, backgroundCheckbox,
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
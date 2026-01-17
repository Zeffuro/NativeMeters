using System;
using System.Numerics;
using FFXIVClientStructs.FFXIV.Component.GUI;
using KamiToolKit.Classes;
using KamiToolKit.Nodes;
using NativeMeters.Configuration;
using NativeMeters.Nodes.Input;

namespace NativeMeters.Nodes.Configuration.Meter;

public sealed class ComponentSettingsNode : VerticalListNode
{
    private readonly RowComponentSettings _component;
    private readonly Action _onChanged;
    private readonly Action _onDeleted;
    private readonly Action _onToggle;

    private readonly VerticalListNode _contentContainer;
    private readonly TextNode _headerText;
    private readonly CircleButtonNode _toggleButton;

    public ComponentSettingsNode(RowComponentSettings component, Action onChanged, Action onDeleted, Action onToggle)
    {
        _component = component;
        _onChanged = onChanged;
        _onDeleted = onDeleted;
        _onToggle = onToggle;

        FitContents = true;
        ItemSpacing = 4.0f;

        // 1. Header
        var header = new HorizontalListNode {
            Size = new Vector2(Width, 24),
            ItemSpacing = 8.0f,
        };
        AddNode(header);

        _toggleButton = new CircleButtonNode {
            Size = new Vector2(20, 20),
            Icon = ButtonIcon.RightArrow,
            OnClick = ToggleExpand
        };
        header.AddNode(_toggleButton);

        _headerText = new TextNode {
            String = $"{_component.Type} - {_component.Name}",
            Size = new Vector2(Width - 30, 24),
            AlignmentType = AlignmentType.Left,
            TextColor = ColorHelper.GetColor(12)
        };
        header.AddNode(_headerText);

        // 2. Content (Indented)
        _contentContainer = new VerticalListNode {
            ItemSpacing = 4.0f,
            FitContents = true,
            X = 24.0f, // Indent
            IsVisible = false
        };
        AddNode(_contentContainer);

        Initialize();
    }

    private void ToggleExpand()
    {
        _contentContainer.IsVisible = !_contentContainer.IsVisible;
        _toggleButton.Icon = _contentContainer.IsVisible ? ButtonIcon.ArrowDown : ButtonIcon.RightArrow;

        // This order is vital: Child first, then bubble up
        RecalculateLayout();
        _onToggle?.Invoke();
    }

    private void Initialize()
    {
        _contentContainer.AddNode(new LabeledTextInputNode {
            LabelText = "Name: ",
            Text = _component.Name,
            OnInputComplete = val => {
                _component.Name = val.ToString();
                _headerText.String = $"{_component.Type} - {val}";
                _onChanged();
            },
            Size = new Vector2(Width - 40, 28)
        });

        _contentContainer.AddNode(new LabeledTextInputNode {
            LabelText = "Data Source: ",
            Text = _component.DataSource,
            OnInputComplete = val => { _component.DataSource = val.ToString(); _onChanged(); },
            Size = new Vector2(Width - 40, 28)
        });

        var posRow = new HorizontalListNode { Size = new Vector2(Width - 40, 30), ItemSpacing = 8.0f };
        posRow.AddNode(new LabeledNumericInputNode {
            LabelText = "X:", Size = new Vector2(100, 28), Value = (int)_component.Position.X,
            OnValueUpdate = val => { _component.Position = _component.Position with { X = (float)val }; _onChanged(); }
        });
        posRow.AddNode(new LabeledNumericInputNode {
            LabelText = "Y:", Size = new Vector2(100, 28), Value = (int)_component.Position.Y,
            OnValueUpdate = val => { _component.Position = _component.Position with { Y = (float)val }; _onChanged(); }
        });
        _contentContainer.AddNode(posRow);

        var sizeRow = new HorizontalListNode { Size = new Vector2(Width - 40, 30), ItemSpacing = 8.0f };
        sizeRow.AddNode(new LabeledNumericInputNode {
            LabelText = "W:", Size = new Vector2(100, 28), Value = (int)_component.Size.X,
            OnValueUpdate = val => { _component.Size = _component.Size with { X = (float)val }; _onChanged(); }
        });
        sizeRow.AddNode(new LabeledNumericInputNode {
            LabelText = "H:", Size = new Vector2(100, 28), Value = (int)_component.Size.Y,
            OnValueUpdate = val => { _component.Size = _component.Size with { Y = (float)val }; _onChanged(); }
        });
        _contentContainer.AddNode(sizeRow);

        if (_component.Type == MeterComponentType.Text) {
            _contentContainer.AddNode(new LabeledNumericInputNode {
                LabelText = "Font Size: ", Value = (int)_component.FontSize,
                OnValueUpdate = val => { _component.FontSize = (uint)val; _onChanged(); }
            });
        }

        _contentContainer.AddNode(new LabeledNumericInputNode {
            LabelText = "Z-Index: ", Value = _component.ZIndex,
            OnValueUpdate = val => { _component.ZIndex = val; _onChanged(); }
        });

        _contentContainer.AddNode(new TextButtonNode {
            String = "Delete Component",
            Size = new Vector2(Width - 40, 24),
            Color = ColorHelper.GetColor(17),
            OnClick = _onDeleted
        });
    }
}
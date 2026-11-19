using System;
using System.Collections.Generic;
using System.Linq;
using System.Numerics;
using FFXIVClientStructs.FFXIV.Component.GUI;
using KamiToolKit.Enums;
using KamiToolKit.Nodes;

namespace NativeMeters.Nodes.Configuration.Setup;

internal sealed class SetupChoiceNode : ResNode, IConfigurationNavigationNode
{
    public const float RowHeight = 28;

    private const float LabelWidth = 108;
    private const float ArrowSize = 22;
    private const float Gap = 4;
    private readonly LabelTextNode label;
    private readonly CircleButtonNode previous;
    private readonly DropDownNode<int> dropdown;
    private readonly CircleButtonNode next;
    private readonly Action<int> changed;
    private int index;

    public SetupChoiceNode(string title, IReadOnlyList<string> labels, int selected, Action<int> changed)
    {
        this.changed = changed;
        index = selected;
        label = new LabelTextNode { String = title, TextFlags = TextFlags.Ellipsis };
        previous = new CircleButtonNode
        {
            Icon = CircleButtonIcon.LeftArrow,
            TextTooltip = $"Previous: {title}",
            OnClick = () => Step(-1),
        };
        dropdown = new DropDownNode<int>
        {
            GetLabelFunction = option => labels[option],
            Options = Enumerable.Range(0, labels.Count).ToList(),
            SelectedOption = index,
            OnOptionSelected = Select,
        };
        next = new CircleButtonNode
        {
            Icon = CircleButtonIcon.RightArrow,
            TextTooltip = $"Next: {title}",
            OnClick = () => Step(1),
        };
        label.AttachNode(this);
        previous.AttachNode(this);
        dropdown.AttachNode(this);
        next.AttachNode(this);
        Height = RowHeight;
    }

    public bool IsEnabled
    {
        get => dropdown.IsEnabled;
        set
        {
            if (dropdown.IsEnabled == value) return;
            if (!value) Collapse();
            previous.IsEnabled = dropdown.IsEnabled = next.IsEnabled = value;
            Alpha = value ? 1f : 0.45f;
        }
    }

    public void SetTooltip(string text) => dropdown.TextTooltip = text;

    public void Collapse() => dropdown.Collapse(playSoundEffect: false);

    public IEnumerable<ConfigurationNavigationTarget> GetNavigationTargets()
    {
        if (!IsEnabled) yield break;
        yield return ConfigurationNavigationTarget.From(previous);
        yield return ConfigurationNavigationTarget.From(dropdown);
        yield return ConfigurationNavigationTarget.From(next);
    }

    protected override void OnSizeChanged()
    {
        base.OnSizeChanged();
        if (Width <= LabelWidth + (ArrowSize + Gap) * 2) return;
        ConfigurationLayout.SetSize(label, new Vector2(LabelWidth, Height));
        var arrowY = (Height - ArrowSize) / 2;
        previous.Position = new Vector2(LabelWidth, arrowY);
        ConfigurationLayout.SetSize(previous, new Vector2(ArrowSize));
        next.Position = new Vector2(Width - ArrowSize, arrowY);
        ConfigurationLayout.SetSize(next, new Vector2(ArrowSize));
        dropdown.Position = new Vector2(LabelWidth + ArrowSize + Gap, 0);
        ConfigurationLayout.SetSize(dropdown, new Vector2(Math.Max(0, next.X - dropdown.X - Gap), Height));
    }

    private void Step(int offset)
    {
        if (!IsEnabled) return;
        Collapse();
        var count = dropdown.Options.Count;
        var selected = (index + offset + count) % count;
        dropdown.SelectedOption = selected;
        Select(selected);
    }

    private void Select(int selected)
    {
        if (index == selected) return;
        index = selected;
        changed(selected);
    }
}

using System;
using System.Numerics;
using FFXIVClientStructs.FFXIV.Component.GUI;
using KamiToolKit.Classes;
using KamiToolKit.Nodes;
using NativeMeters.Nodes.Input;

namespace NativeMeters.Nodes.Configuration.Setup;

internal static class SetupControls
{
    public const float TextHeight = 20;
    public const float CheckHeight = 24;
    public const float Spacing = 4;
    public const float Indent = 16;
    public const float DialogWidth = 660;
    public const float DialogHeight = 596;
    public const float DialogInset = 12;
    public const float FooterInset = 36;
    public const float FooterClearance = 10;
    public const float ButtonWidth = 112;

    public static TabbedVerticalListNode Page(float width)
        => new() { Width = width, FitWidth = true, ItemSpacing = Spacing, TabSize = Indent };

    public static TextButtonNode FooterButton(string text, int navIndex, float width, Action click)
        => new() { Size = new Vector2(width, 27), String = text, NavIndex = navIndex, OnClick = click };

    public static TextNode Line(string text)
        => new()
        {
            Height = TextHeight,
            FontType = FontType.Axis,
            FontSize = 14,
            LineSpacing = 14,
            AlignmentType = AlignmentType.Left,
            TextFlags = TextFlags.Ellipsis,
            TextColor = ColorHelper.GetColor(8),
            TextOutlineColor = ColorHelper.GetColor(7),
            String = text,
        };

    public static CheckboxRowNode Check(string text, bool value, Action<bool> changed)
        => new() { Height = CheckHeight, String = text, IsChecked = value, OnClick = changed };

    public static CategoryTextNode Section(string text)
        => new() { TextFlags = TextFlags.Ellipsis, Height = TextHeight, String = text };
}

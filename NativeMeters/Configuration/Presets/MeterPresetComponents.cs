using System;
using System.Collections.Generic;
using System.Numerics;
using FFXIVClientStructs.FFXIV.Component.GUI;
using KamiToolKit.Classes;
using NativeMeters.Data.Stats;
using NativeMeters.Rendering;

namespace NativeMeters.Configuration.Presets;

internal static class MeterPresetComponents
{
    internal const int DefaultCombatants = 8;
    internal const float CastGaugeFillInset = 6f / 160f;
    internal const float ToDoGaugeFillInset = 2f / 44f;
    internal static Vector4 Ink => ColorHelper.GetColor(50);
    internal static Vector4 Muted => ColorHelper.GetColor(50) with { W = 0.75f };
    internal static Vector4 Accent => ColorHelper.GetColor(8);
    internal static Vector4 CardTint => Vector4.Lerp(Vector4.One, ColorHelper.GetColor(51), 0.25f) with { W = 0.92f };

    internal static void SetLayout(
        MeterSettings settings, MeterLayout layout, float itemWidth, float itemHeight, float spacing,
        float headerHeight, List<ComponentSettings> header, List<ComponentSettings> rows)
    {
        settings.Layout = layout;
        settings.MaxCombatants = DefaultCombatants;
        settings.StatToTrack = StatSelector.DefaultStatSelector;
        settings.RowHeight = itemHeight;
        settings.RowSpacing = spacing;
        settings.HeaderHeight = headerHeight;
        settings.HeaderEnabled = true;
        settings.FooterEnabled = false;
        settings.ShowScrollbar = false;
        settings.ShowWindowBackground = layout == MeterLayout.Vertical;
        settings.BackgroundStyle = MeterBackgroundStyle.Tooltip;
        settings.WindowColor = new Vector4(0, 0, 0, 0.88f);
        settings.HeaderComponents = header;
        settings.RowComponents = rows;
        settings.FooterComponents = [];

        settings.Size = layout == MeterLayout.Horizontal
            ? new Vector2(itemWidth * DefaultCombatants + spacing * (DefaultCombatants - 1), headerHeight + itemHeight)
            : new Vector2(itemWidth, headerHeight + (itemHeight + spacing) * DefaultCombatants);
        settings.EnsureInitialized();
    }

    internal static ComponentSettings Text(
        string name, string value, float x, float y, float width, float height,
        uint fontSize = 14, AlignmentType align = AlignmentType.Left, Vector4? color = null)
        => new()
        {
            Name = name,
            Type = MeterComponentType.Text,
            DataSource = value,
            Position = new Vector2(x, y),
            Size = new Vector2(width, height),
            FontSize = fontSize,
            AlignmentType = align,
            TextColor = color ?? Ink,
            TextFlags = TextFlags.Edge | TextFlags.Ellipsis,
            ColorMode = ColorMode.Static,
            ZIndex = 2,
        };

    internal static ComponentSettings Job(float x, float y, float size)
        => new()
        {
            Name = "Job Icon",
            Type = MeterComponentType.JobIcon,
            Position = new Vector2(x, y),
            Size = new Vector2(size),
            ZIndex = 2,
        };

    internal static ComponentSettings Background(float width, float height, Vector4 color)
        => new()
        {
            Name = "Card Background",
            Type = MeterComponentType.Background,
            Size = new Vector2(width, height),
            TextColor = color,
        };

    internal static ComponentSettings Bar(
        float x, float y, float width, float height, ProgressBarType type = ProgressBarType.ToDo)
        => new()
        {
            Name = "Damage Bar",
            Type = MeterComponentType.ProgressBar,
            DataSource = StatSelector.DefaultStatSelector,
            Position = new Vector2(x, y),
            Size = new Vector2(width, height),
            ColorMode = ColorMode.Job,
            ProgressBarType = type,
            ProgressBarColorTreatment = ProgressBarColorTreatment.Flat,
            BarBackgroundColor = ColorHelper.GetColor(51) with { W = 0.62f },
            ZIndex = 1,
        };

    internal static ComponentSettings? ApplyAppearance(MeterSettings target, CustomMeterBarStyle? barStyle, bool? showBackground,
        ProgressBarType? barType = null, ColorMode? playerColor = null, ProgressBarColorTreatment? barTint = null,
        ColorMode? barColor = null, string? sortBy = null, string? fillStat = null)
    {
        if (showBackground.HasValue) ApplyBackground(target, showBackground.Value);
        if (sortBy != null) target.StatToTrack = StatSelector.NormalizeStatSelector(sortBy);
        var bar = ApplyBar(target, barStyle, barType);
        if (bar != null)
        {
            if (barColor.HasValue) bar.ColorMode = barColor.Value;
            if (barTint.HasValue) bar.ProgressBarColorTreatment = barTint.Value;
            if (fillStat != null) bar.DataSource = StatSelector.NormalizeStatSelector(fillStat);
        }

        if (playerColor is { } color)
        {
            foreach (var component in target.RowComponents)
            {
                if (component is { Type: MeterComponentType.Text } && ColorResolver.IsNameTextComponent(component))
                    component.ColorMode = color;
            }
        }
        return bar;
    }

    private static void ApplyBackground(MeterSettings target, bool showBackground)
    {
        if (!showBackground)
        {
            target.ShowWindowBackground = false;
            target.RowComponents.RemoveAll(component => component?.Type == MeterComponentType.Background);
            target.HeaderComponents.RemoveAll(component => component?.Type == MeterComponentType.Background);
            target.FooterComponents.RemoveAll(component => component?.Type == MeterComponentType.Background);
            return;
        }

        if (target.Layout == MeterLayout.Vertical)
            target.ShowWindowBackground = true;
        else if (!target.ShowWindowBackground && !target.RowComponents.Exists(component => component?.Type == MeterComponentType.Background))
            target.RowComponents.Insert(0, Background(GetRowWidth(target), target.RowHeight, CardTint));
    }

    private static ComponentSettings? ApplyBar(MeterSettings target, CustomMeterBarStyle? style, ProgressBarType? type)
    {
        var rows = target.RowComponents;
        var index = rows.FindIndex(component => component?.Type == MeterComponentType.ProgressBar);
        if (style == CustomMeterBarStyle.None)
        {
            if (index >= 0) rows.RemoveAt(index);
            return null;
        }

        var bar = index >= 0 ? rows[index] : null;
        if (style == null && type == null) return bar;
        var barType = type ?? bar?.ProgressBarType ?? ProgressBarType.ToDo;
        var minimumHeight = GetMinimumBarHeight(barType);

        if (style == null)
        {
            if (bar == null || bar.ProgressBarType == barType) return bar;
            bar.ProgressBarType = barType;
            if (bar.Size.Y > 0 && bar.Size.Y < minimumHeight)
            {
                var bottomPadding = Math.Max(0f, target.RowHeight - bar.Position.Y - bar.Size.Y);
                bar.Size = bar.Size with { Y = minimumHeight };
                EnsureRowHeight(target, bar.Position.Y + minimumHeight + bottomPadding);
            }
            return bar;
        }

        if (style is not (CustomMeterBarStyle.Underline or CustomMeterBarStyle.FullWidth)) return bar;

        var rowWidth = GetRowWidth(target);
        var rowHeight = target.RowHeight;
        if (rowWidth < 1 || rowHeight < 1) return bar;

        var underline = style == CustomMeterBarStyle.Underline;
        var padding = target.Layout == MeterLayout.Horizontal ? 7f : 8f;
        var x = Math.Min(padding, MathF.Floor((rowWidth - 1f) / 2f));
        var width = rowWidth - x * 2f;
        var y = 1f;
        var height = Math.Max(minimumHeight, rowHeight - y * 2f);
        if (underline)
        {
            height = minimumHeight;
            y = Math.Max(0f, rowHeight - height - 1f);
            if (height > 4f)
            {
                foreach (var component in rows)
                {
                    if (component is { Type: MeterComponentType.Text or MeterComponentType.JobIcon or MeterComponentType.Icon or MeterComponentType.Asset })
                        y = Math.Max(y, component.Position.Y + component.Size.Y + 2f);
                }
            }
        }
        EnsureRowHeight(target, y + height + 1f);

        if (bar == null)
        {
            bar = Bar(x, y, width, height, barType);
            rows.Add(bar);
            return bar;
        }

        bar.Position = new Vector2(x, y);
        bar.Size = new Vector2(width, height);
        bar.ProgressBarType = barType;
        return bar;
    }

    internal static float GetMinimumBarHeight(ProgressBarType barType) => barType switch
    {
        ProgressBarType.LimitBreak => 20f,
        ProgressBarType.PartyListHp => 12f,
        ProgressBarType.Cast or ProgressBarType.EnemyCast => 10f,
        _ => 4f,
    };

    private static void EnsureRowHeight(MeterSettings target, float minimumHeight)
    {
        var previousHeight = target.RowHeight;
        if (previousHeight >= minimumHeight) return;

        foreach (var component in target.RowComponents)
        {
            if (component?.Type == MeterComponentType.Background && component.Position.Y == 0f && component.Size.Y == previousHeight)
                component.Size = component.Size with { Y = minimumHeight };
        }
        target.RowHeight = minimumHeight;
        var rowCount = target.Layout == MeterLayout.Horizontal ? 1 : Math.Max(0, target.MaxCombatants);
        target.Size = target.Size with { Y = target.Size.Y + rowCount * (minimumHeight - previousHeight) };
    }

    private static float GetRowWidth(MeterSettings target)
    {
        var width = target.ShowWindowBackground
            && target.BackgroundStyle is MeterBackgroundStyle.Window or MeterBackgroundStyle.WindowFocused
            ? target.Size.X - 16f
            : target.Size.X;
        return target.Layout == MeterLayout.Horizontal ? MeterRowBuilder.GetHorizontalCardWidth(width, target) : width;
    }

    internal static List<ComponentSettings> Header(float width, bool totals, Vector4? durationColor = null)
    {
        var titleWidth = Math.Clamp(width - 110f - (totals ? 352f : 0f), 64f, 380f);
        List<ComponentSettings> components =
        [
            new()
            {
                Name = "Menu Button",
                Type = MeterComponentType.MenuButton,
                Position = new Vector2(6, 2),
                Size = new Vector2(22),
                ZIndex = 2,
            },
            Text("Duration", "[duration]", 34, 2, 66, 22, 14, color: durationColor ?? Accent),
            Text("Encounter Name", "[name]", 106, 2, titleWidth, 22, 15),
        ];

        if (totals)
        {
            components.Add(Text("Total Damage", "Damage  [damage.0]", width - 352, 2, 176, 22,
                14, AlignmentType.Right, Muted));
            components.Add(Text("Party DPS", "Party DPS  [ENCDPS.0]", width - 168, 2, 160, 22,
                14, AlignmentType.Right, Accent));
        }

        return components;
    }
}

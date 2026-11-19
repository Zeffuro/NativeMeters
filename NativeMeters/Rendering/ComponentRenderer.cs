using System;
using System.Numerics;
using FFXIVClientStructs.FFXIV.Component.GUI;
using KamiToolKit.BaseTypes;
using KamiToolKit.Nodes;
using NativeMeters.Configuration;
using NativeMeters.Data.Stats;
using NativeMeters.Extensions;
using NativeMeters.Models;
using NativeMeters.Nodes;
using NativeMeters.Nodes.Components;
using NativeMeters.Tags;
using BackgroundTextNode = NativeMeters.Nodes.BackgroundTextNode;

namespace NativeMeters.Rendering;

public static class ComponentRenderer
{
    public static void Update(NodeBase node, ComponentSettings settings, float containerWidth, object data,
        MeterSettings? meterSettings = null, CombatantTagContext? tagContext = null,
        Func<Func<Combatant, double>, double>? previewMaxStat = null,
        Vector2? componentSize = null, Vector2? componentPosition = null)
    {
        node.IsVisible = true;
        var size = componentSize ?? settings.Size;

        switch (node)
        {
            case BackgroundTextNode textNode:
                textNode.FontSize = data is Combatant fontSizeCombatant
                    ? (int)ColorResolver.GetTextFontSize(fontSizeCombatant, settings, meterSettings)
                    : (int)settings.FontSize;
                textNode.FontType = data is Combatant fontTypeCombatant
                    ? ColorResolver.GetTextFontType(fontTypeCombatant, settings, meterSettings)
                    : settings.FontType;
                var flags = data is Combatant flagsCombatant
                    ? ColorResolver.GetTextFlags(flagsCombatant, settings, meterSettings)
                    : settings.TextFlags;
                textNode.TextFlags = componentSize.HasValue
                    ? (flags & ~TextFlags.AutoAdjustNodeSize) | TextFlags.Ellipsis
                    : flags;
                textNode.TextColor = data is Combatant c ? c.GetColor(settings.ColorMode, settings, meterSettings) : settings.TextColor;
                textNode.TextOutlineColor = data is Combatant outlineCombatant
                    ? ColorResolver.GetTextOutlineColor(outlineCombatant, settings, meterSettings)
                    : settings.TextOutlineColor;
                textNode.AlignmentType = settings.AlignmentType;
                textNode.BackgroundColor = data is Combatant textBackgroundCombatant
                    ? ColorResolver.GetTextBackgroundColor(textBackgroundCombatant, settings, meterSettings)
                    : settings.TextBackgroundColor;
                textNode.ShowBackground = settings.ShowBackground;
                textNode.String = TagEngine.ProcessCached(settings.DataSource, tagContext ?? data, textNode);

                if (size.X > 0) node.Width = size.X;
                if (size.Y > 0) node.Height = size.Y;
                break;

            case AssetImageNode assetNode:
                assetNode.IsVisible = assetNode.ApplyAsset(settings.Asset);
                break;

            case IconImageNode iconNode:
                switch (settings.Type)
                {
                    case MeterComponentType.JobIcon when data is Combatant combatant:
                    {
                        var iconId = combatant.GetIconId(settings.JobIconType);
                        iconNode.IsVisible = iconId != 0;
                        if (iconId != 0) iconNode.IconId = iconId;
                        break;
                    }
                    case MeterComponentType.JobIcon:
                        iconNode.IsVisible = false;
                        break;
                    case MeterComponentType.Icon:
                        iconNode.IconId = settings.IconId;
                        iconNode.IsVisible = settings.IconId != 0;
                        break;
                }
                break;

            case IMeterProgressNode progressNode:
                UpdateProgressNode(node, progressNode, settings, size, containerWidth, data, meterSettings, previewMaxStat);
                break;

            case ProgressNode progressNode:
                UpdateProgressNode(progressNode, settings, size, containerWidth, data, meterSettings, previewMaxStat);
                break;

            case HeaderMenuButtonNode buttonNode:
                buttonNode.Configure(meterSettings, settings.ButtonAction);
                break;

            case HorizontalLineNode:
                break;

            case NineGridNode backgroundNode:
                backgroundNode.Color = data is Combatant backgroundCombatant
                    ? ColorResolver.GetRowBackgroundColor(backgroundCombatant, settings, meterSettings)
                    : settings.TextColor;
                break;
        }

        if ((node is not BackgroundTextNode || size.X > 0) && node is not ProgressNode && node is not IMeterProgressNode)
        {
            float componentWidth = size.X <= 0 ? containerWidth : size.X;
            SetSize(node, size with { X = componentWidth });
        }

        node.Position = componentPosition ?? settings.Position;
    }

    private static void SetSize(NodeBase node, Vector2 size)
    {
        var nativeSize = new Vector2((ushort)size.X, (ushort)size.Y);
        if (node.Size != nativeSize) node.Size = size;
    }

    private static void UpdateProgressNode(ProgressNode progressNode, ComponentSettings settings, Vector2 componentSize,
        float containerWidth, object data, MeterSettings? meterSettings, Func<Func<Combatant, double>, double>? previewMaxStat)
    {
        if (componentSize.X > 0 || componentSize.Y > 0)
        {
            float componentWidth = componentSize.X <= 0 ? containerWidth : componentSize.X;
            var size = new Vector2(componentWidth, componentSize.Y > 0 ? componentSize.Y : progressNode.Height);
            SetSize(progressNode, size);
        }

        if (data is Combatant comb)
        {
            var statName = StatSelector.NormalizeStatSelector(settings.DataSource);
            var selector = StatSelector.GetStatSelector(statName);
            double maxStat = previewMaxStat?.Invoke(selector) ?? System.ActiveMeterService.GetMaxCombatantStat(selector);
            progressNode.Progress = ViewUtils.CalculateProgressRatio(selector(comb), maxStat > 0 ? maxStat : 1.0);
            progressNode.BarColor = comb.GetColor(settings.ColorMode, settings, meterSettings);
            progressNode.BackgroundColor = ColorResolver.GetBarBackgroundColor(comb, settings, meterSettings);
        }
    }

    private static void UpdateProgressNode(NodeBase node, IMeterProgressNode progressNode, ComponentSettings settings,
        Vector2 componentSize, float containerWidth, object data, MeterSettings? meterSettings,
        Func<Func<Combatant, double>, double>? previewMaxStat)
    {
        if (componentSize.X > 0 || componentSize.Y > 0)
        {
            float componentWidth = componentSize.X <= 0 ? containerWidth : componentSize.X;
            var size = new Vector2(componentWidth, componentSize.Y > 0 ? componentSize.Y : node.Height);
            if (node.Size != size) node.Size = size;
        }

        if (data is Combatant comb)
        {
            var statName = StatSelector.NormalizeStatSelector(settings.DataSource);
            var selector = StatSelector.GetStatSelector(statName);
            double maxStat = previewMaxStat?.Invoke(selector) ?? System.ActiveMeterService.GetMaxCombatantStat(selector);
            progressNode.Progress = ViewUtils.CalculateProgressRatio(selector(comb), maxStat > 0 ? maxStat : 1.0);
            progressNode.ColorTreatment = settings.ProgressBarColorTreatment;
            progressNode.FillRightToLeft = settings.ProgressBarFillRightToLeft;
            progressNode.BarColor = comb.GetColor(settings.ColorMode, settings, meterSettings);
            progressNode.BackgroundColor = ColorResolver.GetBarBackgroundColor(comb, settings, meterSettings);
        }
    }
}

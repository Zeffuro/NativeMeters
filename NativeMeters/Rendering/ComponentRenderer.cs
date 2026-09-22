using System.Numerics;
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
    public static void Update(NodeBase node, ComponentSettings settings, float containerWidth, object data, MeterSettings? meterSettings = null)
    {
        node.IsVisible = true;

        switch (node)
        {
            case BackgroundTextNode textNode:
                textNode.FontSize = data is Combatant fontSizeCombatant
                    ? (int)ColorResolver.GetTextFontSize(fontSizeCombatant, settings, meterSettings)
                    : (int)settings.FontSize;
                textNode.FontType = data is Combatant fontTypeCombatant
                    ? ColorResolver.GetTextFontType(fontTypeCombatant, settings, meterSettings)
                    : settings.FontType;
                textNode.TextFlags = data is Combatant flagsCombatant
                    ? ColorResolver.GetTextFlags(flagsCombatant, settings, meterSettings)
                    : settings.TextFlags;
                textNode.TextColor = data is Combatant c ? c.GetColor(settings.ColorMode, settings, meterSettings) : settings.TextColor;
                textNode.TextOutlineColor = data is Combatant outlineCombatant
                    ? ColorResolver.GetTextOutlineColor(outlineCombatant, settings, meterSettings)
                    : settings.TextOutlineColor;
                textNode.AlignmentType = settings.AlignmentType;
                textNode.BackgroundColor = data is Combatant textBackgroundCombatant
                    ? ColorResolver.GetTextBackgroundColor(textBackgroundCombatant, settings, meterSettings)
                    : settings.TextBackgroundColor;
                textNode.ShowBackground = settings.ShowBackground;
                textNode.String = TagEngine.Process(settings.DataSource, data);

                if (settings.Size.X > 0) node.Width = settings.Size.X;
                if (settings.Size.Y > 0) node.Height = settings.Size.Y;
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
                UpdateProgressNode(node, progressNode, settings, containerWidth, data, meterSettings);
                break;

            case ProgressNode progressNode:
                UpdateProgressNode(progressNode, settings, containerWidth, data, meterSettings);
                break;

            case HeaderMenuButtonNode:
            case HorizontalLineNode:
                break;

            case NineGridNode backgroundNode:
                backgroundNode.Color = data is Combatant backgroundCombatant
                    ? ColorResolver.GetRowBackgroundColor(backgroundCombatant, settings, meterSettings)
                    : settings.TextColor;
                break;
        }

        if ((node is not BackgroundTextNode || settings.Size.X > 0) && node is not ProgressNode && node is not IMeterProgressNode)
        {
            float componentWidth = settings.Size.X <= 0 ? containerWidth : settings.Size.X;
            node.Size = settings.Size with { X = componentWidth };
        }

        node.Position = settings.Position;
    }

    private static void UpdateProgressNode(ProgressNode progressNode, ComponentSettings settings, float containerWidth, object data, MeterSettings? meterSettings)
    {
        if (settings.Size.X > 0 || settings.Size.Y > 0)
        {
            float componentWidth = settings.Size.X <= 0 ? containerWidth : settings.Size.X;
            progressNode.Size = new Vector2(componentWidth, settings.Size.Y > 0 ? settings.Size.Y : progressNode.Height);
        }

        if (data is Combatant comb)
        {
            var statName = StatSelector.NormalizeStatSelector(settings.DataSource);
            var selector = StatSelector.GetStatSelector(statName);
            double maxStat = System.ActiveMeterService.GetMaxCombatantStat(selector);
            progressNode.Progress = ViewUtils.CalculateProgressRatio(selector(comb), maxStat > 0 ? maxStat : 1.0);
            progressNode.BarColor = comb.GetColor(settings.ColorMode, settings, meterSettings);
            progressNode.BackgroundColor = ColorResolver.GetBarBackgroundColor(comb, settings, meterSettings);
        }
    }

    private static void UpdateProgressNode(NodeBase node, IMeterProgressNode progressNode, ComponentSettings settings, float containerWidth, object data, MeterSettings? meterSettings)
    {
        if (settings.Size.X > 0 || settings.Size.Y > 0)
        {
            float componentWidth = settings.Size.X <= 0 ? containerWidth : settings.Size.X;
            node.Size = new Vector2(componentWidth, settings.Size.Y > 0 ? settings.Size.Y : node.Height);
        }

        if (data is Combatant comb)
        {
            var statName = StatSelector.NormalizeStatSelector(settings.DataSource);
            var selector = StatSelector.GetStatSelector(statName);
            double maxStat = System.ActiveMeterService.GetMaxCombatantStat(selector);
            progressNode.Progress = ViewUtils.CalculateProgressRatio(selector(comb), maxStat > 0 ? maxStat : 1.0);
            progressNode.ColorTreatment = settings.ProgressBarColorTreatment;
            progressNode.FillRightToLeft = settings.ProgressBarFillRightToLeft;
            progressNode.BarColor = comb.GetColor(settings.ColorMode, settings, meterSettings);
            progressNode.BackgroundColor = ColorResolver.GetBarBackgroundColor(comb, settings, meterSettings);
        }
    }
}

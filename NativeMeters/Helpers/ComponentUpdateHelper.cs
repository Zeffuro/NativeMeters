using FFXIVClientStructs.FFXIV.Component.GUI;
using KamiToolKit;
using KamiToolKit.Nodes;
using NativeMeters.Configuration;
using NativeMeters.Extensions;
using NativeMeters.Models;
using NativeMeters.Nodes;

namespace NativeMeters.Helpers;

public static class ComponentUpdateHelper
{
    public static void Update(NodeBase node, ComponentSettings settings, float containerWidth, object data)
    {
        node.IsVisible = true;

        switch (node)
        {
            case BackgroundTextNode textNode:
                textNode.FontSize = (int)settings.FontSize;
                textNode.FontType = settings.FontType;
                textNode.TextFlags = settings.TextFlags;
                textNode.TextColor = (settings.UseJobColor && data is Combatant c) ? c.GetColor() : settings.TextColor;
                textNode.TextOutlineColor = settings.TextOutlineColor;
                textNode.BackgroundColor = settings.TextBackgroundColor;
                textNode.ShowBackground = settings.ShowBackground;
                textNode.String = TagProcessor.Process(settings.DataSource, data);

                if (settings.Size.X > 0) node.Width = settings.Size.X;
                if (settings.Size.Y > 0) node.Height = settings.Size.Y;
                break;

            case IconImageNode iconNode:
                if (data is Combatant combatant) {
                    var iconId = combatant.GetIconId(settings.JobIconType);
                    iconNode.IsVisible = iconId != 0;
                    if (iconId != 0) iconNode.IconId = iconId;
                }
                break;

            case ProgressNode progressNode:
                if (data is Combatant comb) {
                    var statName = string.IsNullOrWhiteSpace(settings.DataSource) ? "ENCDPS" : settings.DataSource;
                    var selector = CombatantStatHelpers.GetStatSelector(statName);
                    double maxStat = System.ActiveMeterService.GetMaxCombatantStat(selector);
                    progressNode.Progress = MeterUtil.CalculateProgressRatio(selector(comb), maxStat > 0 ? maxStat : 1.0);
                    progressNode.BarColor = settings.UseJobColor ? comb.GetColor() : settings.BarColor;
                    progressNode.BackgroundColor = settings.BarBackgroundColor;
                }
                break;

            case NineGridNode backgroundNode:
                backgroundNode.Color = settings.TextColor;
                break;
        }

        if (node is not BackgroundTextNode || settings.Size.X > 0)
        {
            float componentWidth = settings.Size.X <= 0 ? containerWidth : settings.Size.X;
            node.Size = settings.Size with { X = componentWidth };
        }

        float x = settings.AlignmentType switch {
            AlignmentType.TopRight or AlignmentType.Right or AlignmentType.BottomRight
                => containerWidth - node.Width - settings.Position.X,
            AlignmentType.Top or AlignmentType.Center or AlignmentType.Bottom
                => (containerWidth / 2.0f) - (node.Width / 2.0f) + settings.Position.X,
            _ => settings.Position.X
        };
        node.Position = settings.Position with { X = x };
    }
}

using System.Numerics;
using KamiToolKit;
using KamiToolKit.Nodes;
using NativeMeters.Configuration;
using NativeMeters.Data.Stats;
using NativeMeters.Extensions;
using NativeMeters.Models;
using NativeMeters.Nodes;
using NativeMeters.Tags;

namespace NativeMeters.Rendering;

public static class ComponentRenderer
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
                textNode.TextColor = data is Combatant c ? c.GetColor(settings.ColorMode, settings) : settings.TextColor;
                textNode.TextOutlineColor = settings.TextOutlineColor;
                textNode.AlignmentType = settings.AlignmentType;
                textNode.BackgroundColor = settings.TextBackgroundColor;
                textNode.ShowBackground = settings.ShowBackground;
                textNode.String = TagEngine.Process(settings.DataSource, data);

                if (settings.Size.X > 0) node.Width = settings.Size.X;
                if (settings.Size.Y > 0) node.Height = settings.Size.Y;
                break;

            case IconImageNode iconNode:
                if (data is Combatant combatant)
                {
                    var iconId = combatant.GetIconId(settings.JobIconType);
                    iconNode.IsVisible = iconId != 0;
                    if (iconId != 0) iconNode.IconId = iconId;
                }
                break;

            case ProgressNode progressNode:
                if (settings.Size.X > 0 || settings.Size.Y > 0)
                {
                    float componentWidth = settings.Size.X <= 0 ? containerWidth : settings.Size.X;
                    progressNode.Size = new Vector2(componentWidth, settings.Size.Y > 0 ? settings.Size.Y : progressNode.Height);
                }
                if (data is Combatant comb)
                {
                    var statName = string.IsNullOrWhiteSpace(settings.DataSource) ? "ENCDPS" : settings.DataSource;
                    var selector = StatSelector.GetStatSelector(statName);
                    double maxStat = System.ActiveMeterService.GetMaxCombatantStat(selector);
                    progressNode.Progress = ViewUtils.CalculateProgressRatio(selector(comb), maxStat > 0 ? maxStat : 1.0);
                    progressNode.BarColor = comb.GetColor(settings.ColorMode, settings);
                    progressNode.BackgroundColor = settings.BarBackgroundColor;
                }
                break;

            case NineGridNode backgroundNode:
                backgroundNode.Color = settings.TextColor;
                break;
        }

        if ((node is not BackgroundTextNode || settings.Size.X > 0) && node is not ProgressNode)
        {
            float componentWidth = settings.Size.X <= 0 ? containerWidth : settings.Size.X;
            node.Size = settings.Size with { X = componentWidth };
        }

        node.Position = settings.Position;
    }
}

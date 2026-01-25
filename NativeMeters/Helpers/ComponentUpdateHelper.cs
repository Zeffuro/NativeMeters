using System.Numerics;
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
                textNode.TextColor = data is Combatant c ? GetColorForCombatant(c, settings) : settings.TextColor;
                textNode.TextOutlineColor = settings.TextOutlineColor;
                textNode.AlignmentType = settings.AlignmentType;
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
                if (settings.Size.X > 0 || settings.Size.Y > 0) {
                    float componentWidth = settings.Size.X <= 0 ? containerWidth : settings.Size.X;
                    progressNode.Size = new Vector2(componentWidth, settings.Size.Y > 0 ? settings.Size.Y : progressNode.Height);
                }
                if (data is Combatant comb) {
                    var statName = string.IsNullOrWhiteSpace(settings.DataSource) ? "ENCDPS" : settings.DataSource;
                    var selector = CombatantStatHelpers.GetStatSelector(statName);
                    double maxStat = System.ActiveMeterService.GetMaxCombatantStat(selector);
                    progressNode.Progress = MeterUtil.CalculateProgressRatio(selector(comb), maxStat > 0 ? maxStat : 1.0);
                    progressNode.BarColor = GetColorForCombatant(comb, settings);
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

    public static Vector4 GetColorForCombatant(Combatant combatant, ComponentSettings settings)
    {
        if (settings.ColorMode == ColorMode.Static)
            return settings.BarColor;

        var config = System.Config.General;

        if (combatant.Name.Equals("Limit Break", global::System.StringComparison.OrdinalIgnoreCase))
            return config.OtherColor;

        if (settings.ColorMode == ColorMode.Role)
        {
            return combatant.Job.Role switch
            {
                1 => config.TankColor,
                4 => config.HealerColor,
                2 or 3 => config.DpsColor, // Melee or Ranged
                _ => config.OtherColor
            };
        }

        if (config.JobColors.TryGetValue(combatant.Job.RowId, out var jobColor))
            return jobColor;

        return config.OtherColor;
    }
}

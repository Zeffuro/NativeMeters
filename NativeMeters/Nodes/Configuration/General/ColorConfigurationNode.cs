using System;
using System.Numerics;
using System.Linq;
using KamiToolKit.Nodes;
using NativeMeters.Nodes.Color;
using NativeMeters.Nodes.Configuration;
using NativeMeters.Nodes.LayoutNodes;
using NativeMeters.Services;
using Lumina.Excel.Sheets;
using NativeMeters.Configuration.Persistence;
using NativeMeters.Models;

namespace NativeMeters.Nodes.Configuration.General;

public sealed class ColorConfigurationNode : ScrollingNode<VerticalListNode>
{
    private const int FirstContentNavIndex = 6;

    private readonly CategoryNode roleCategory;
    private readonly CategoryNode jobCategory;

    public int TabBarNavIndex { get; set; } = 4;

    public ColorConfigurationNode()
    {
        var config = System.Config.General;
        ContentNode.ItemSpacing = 10;
        ContentNode.FitContents = true;

        roleCategory = new CategoryNode
        {
            String = "Role Colors",
            IsCollapsed = false,
            HeaderHeight = 28,
            OnToggle = RecalculateSizes
        };
        roleCategory.AddTab();

        var roleDefinitions = new (RoleType Role, string Label, Vector4 Current, Vector4 Default, Action<Vector4> Setter)[]
        {
            (RoleType.Tank, "Tanks", config.TankColor, new Vector4(0.18f, 0.43f, 0.71f, 1.0f), c => config.TankColor = c),
            (RoleType.Healer, "Healers", config.HealerColor, new Vector4(0.18f, 0.58f, 0.18f, 1.0f), c => config.HealerColor = c),
            (RoleType.DPS, "DPS", config.DpsColor, new Vector4(0.62f, 0.16f, 0.16f, 1.0f), c => config.DpsColor = c),
            (RoleType.Other, "Other/Limit Break", config.OtherColor, JobColorMaps.DefaultColors[0], c => config.OtherColor = c)
        };

        foreach (var def in roleDefinitions)
        {
            var roleRow = new HorizontalListNode { Size = new Vector2(400, 30), ItemSpacing = 6 };

            roleRow.AddNode(new RoleIconNode
            {
                Role = def.Role,
                Size = new Vector2(24, 24),
                Y = 3,
            });

            roleRow.AddNode(new ColorInputRow
            {
                Label = def.Label,
                Size = new Vector2(300, 28),
                DefaultColor = def.Default,
                CurrentColor = def.Current,
                OnColorConfirmed = c => { def.Setter(c); ConfigRepository.Save(System.Config); },
                OnColorPreviewed = c => def.Setter(c),
                OnColorCanceled = c => def.Setter(c),
            });

            roleCategory.AddNode(roleRow);
        }


        jobCategory = new CategoryNode {
            String = "Job Colors",
            IsCollapsed = false,
            HeaderHeight = 28,
            OnToggle = RecalculateSizes
        };
        jobCategory.AddTab();

        var jobs = Service.DataManager.GetExcelSheet<ClassJob>()
            .Where(classJob => classJob.JobIndex > 0 && classJob.Role != 0)
            .OrderBy(GetJobPriority)
            .ThenBy(j => j.RowId)
            .ToList();

        foreach (var job in jobs)
        {
            var jobId = job.RowId;
            var jobRow = new HorizontalListNode
            {
                Size = new Vector2(400, 30),
                ItemSpacing = 8
            };

            jobRow.AddNode(new IconImageNode {
                Size = new Vector2(24, 24),
                IconId = JobIconMaps.GetIcon(jobId, JobIconType.Default),
                FitTexture = true
            });

            jobRow.AddNode(new ColorInputRow {
                Label = job.NameEnglish.ToString(),
                Size = new Vector2(300, 28),
                DefaultColor = JobColorMaps.DefaultColors.TryGetValue(jobId, out var def) ? def : config.OtherColor,
                CurrentColor = config.JobColors.TryGetValue(jobId, out var color) ? color : config.OtherColor,
                OnColorConfirmed = c => { config.JobColors[jobId] = c; ConfigRepository.Save(System.Config); },
                OnColorPreviewed = c => config.JobColors[jobId] = c,
                OnColorCanceled = c => config.JobColors[jobId] = c,
            });

            jobCategory.AddNode(jobRow);
        }

        ContentNode.AddNode([roleCategory, jobCategory]);

        RecalculateConfigurationLayout();
    }

    private static int GetJobPriority(ClassJob job) {
        if (job.RowId == 36) return 6; // Blue Mage
        return job.Role switch {
            1 => 1, // Tank
            4 => 2, // Healer
            2 => 3, // Melee
            3 => job.PrimaryStat == 4 ? 5 : 4, // Caster(4) vs Phys Range(5)
            _ => 7,
        };
    }

    protected override void OnSizeChanged()
    {
        base.OnSizeChanged();

        float listWidth = Math.Max(0, Width - 16.0f);
        ContentNode.Width = listWidth;

        if (roleCategory != null) roleCategory.Width = listWidth;
        if (jobCategory != null) jobCategory.Width = listWidth;

        RecalculateConfigurationLayout();
    }

    private void RecalculateConfigurationLayout()
    {
        LayoutRecalculation.RecalculateBottomUp(ContentNode);
        LayoutRecalculation.UpdateScrollParams(this);
        ConfigurationNavigation.Apply(ContentNode, FirstContentNavIndex, TabBarNavIndex, TabBarNavIndex);
    }
}

using KamiToolKit.Nodes;
using NativeMeters.Helpers;
using NativeMeters.Models;
using NativeMeters.Services;

namespace NativeMeters.Nodes.LayoutNodes;

public sealed class MeterRowNode : HorizontalListNode
{
    private CastBarProgressBarNode barProgressNode;
    private TextNode nameTextNode;
    public MeterRowNode()
    {
        X = 0;
        Y = 0;
        Width = 500;
        Height = 36;
        IsVisible = true;

        barProgressNode = new CastBarProgressBarNode
        {
            NodeId = 2,
            X = 0,
            Y = 0,
            Width = 200,
            Height = 20,
            IsVisible = true,
        };
        AddNode(barProgressNode);

        nameTextNode = new TextNode
        {
            NodeId = 3,
            X = 210,
            Y = 0,
            Width = 180,
            Height = 20,
            FontSize = 14,
            IsVisible = true,
            String = "Combatant?.Name"
        };
        AddNode(nameTextNode);
    }

    public required Combatant Combatant
    {
        get;
        set
        {
            field = value;
            nameTextNode.String = field.Name;
            barProgressNode.Progress = MeterUtil.CalculateProgressRatio(field.ENCDPS, Encounter?.ENCDPS ?? 1.0);
            Service.Logger.Info($"Set Combatant: {field.Name} with ENCDPS: {field.ENCDPS} and total: {Encounter?.ENCDPS} and Progress: {barProgressNode.Progress}");
        }
    }

    public required Encounter Encounter
    {
        get;
        set;
    }
}
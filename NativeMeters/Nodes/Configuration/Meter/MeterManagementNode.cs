using System;
using System.Collections.Generic;
using System.Linq;
using System.Numerics;
using FFXIVClientStructs.FFXIV.Component.GUI;
using KamiToolKit.Nodes;
using KamiToolKit.Premade.Nodes;
using NativeMeters.Configuration;
using NativeMeters.Addons;
using NativeMeters.Configuration.Persistence;
using NativeMeters.Configuration.Presets;

namespace NativeMeters.Nodes.Configuration.Meter;

public class MeterManagementNode : SimpleComponentNode
{
    private readonly ModifyListNode<MeterWrapper, MeterListItemNode>? selectionListNode;
    private readonly MeterConfigurationNode? configNode;
    private readonly TextNode? nothingSelectedTextNode;
    private readonly List<MeterWrapper> meterWrappers;

    public MeterManagementNode()
    {
        meterWrappers = System.Config.Meters.Select(meterSettings => new MeterWrapper(meterSettings)).ToList();

        selectionListNode = new ModifyListNode<MeterWrapper, MeterListItemNode>
        {
            Position = Vector2.Zero,
            Size = new Vector2(200.0f, 400.0f),
            Options = meterWrappers,
            SelectionChanged = OnOptionChanged,
            AddNewEntry = OnAddNewMeter,
            RemoveEntry = OnRemoveMeter,
            SortOptions = [ "Alphabetical" ],
            ItemComparer = (left, right, mode) => left.Compare(right, mode),
            IsSearchMatch = (data, search) => data.GetLabel().Contains(search, StringComparison.OrdinalIgnoreCase),
        };
        selectionListNode.AttachNode(this);

        configNode = new MeterConfigurationNode
        {
            Position = new Vector2(210.0f, 0.0f),
            IsVisible = false,
        };
        configNode.AttachNode(this);

        nothingSelectedTextNode = new TextNode
        {
            Position = configNode.Position,
            AlignmentType = AlignmentType.Center,
            String = "Select a meter or create a new one.",
        };
        nothingSelectedTextNode.AttachNode(this);
    }

    protected override void OnSizeChanged()
    {
        base.OnSizeChanged();
        if (selectionListNode != null) selectionListNode.Height = Height;
        if (configNode != null)
        {
            configNode.Size = Size - new Vector2(210.0f, 0.0f);
            nothingSelectedTextNode!.Size = configNode.Size;
        }
    }

    private void OnOptionChanged(MeterWrapper? newOption)
    {
        configNode!.IsVisible = newOption != null;
        nothingSelectedTextNode!.IsVisible = newOption == null;
        configNode.ConfigurationOption = newOption;
    }

    private void OnAddNewMeter()
    {
        var newMeter = new MeterSettings {
            Name = $"New Meter {System.Config.Meters.Count + 1}",
        };
        MeterPresets.ApplyDefaultStylish(newMeter);
        System.Config.Meters.Add(newMeter);
        ConfigRepository.Save(System.Config);
        var wrapper = new MeterWrapper(newMeter);
        meterWrappers.Add(wrapper);
        selectionListNode?.RefreshList();
        System.OverlayManager.Setup();
    }

    private void OnRemoveMeter(MeterWrapper wrapper)
    {
        System.Config.Meters.Remove(wrapper.MeterSettings);
        ConfigRepository.Save(System.Config);
        meterWrappers.Remove(wrapper);
        selectionListNode?.RefreshList();
        System.OverlayManager.Setup();
        if (ReferenceEquals(configNode?.ConfigurationOption, wrapper)) OnOptionChanged(null);
    }
}

using System.Collections.Generic;
using System.Linq;
using System.Numerics;
using FFXIVClientStructs.FFXIV.Component.GUI;
using KamiToolKit;
using KamiToolKit.Classes;
using KamiToolKit.Nodes;
using KamiToolKit.Premade.Nodes;
using NativeMeters.Configuration;
using NativeMeters.Nodes.Configuration.Meter;

namespace NativeMeters.Addons;

public class _AddonMeterConfigurationWindow : NativeAddon
{
    private ModifyListNode<MeterWrapper>? _selectionListNode;
    private MeterConfigurationNode? _configNode;
    private TextNode? _nothingSelectedTextNode;
    private List<MeterWrapper> _meterWrappers = new();

    protected override unsafe void OnSetup(AtkUnitBase* addon)
    {
        _meterWrappers = System.Config.Meters.Select(m => new MeterWrapper(m)).ToList();

        _selectionListNode = new ModifyListNode<MeterWrapper>
        {
            Position = ContentStartPosition,
            Size = new Vector2(200.0f, ContentSize.Y),
            SelectionOptions = _meterWrappers,
            OnOptionChanged = OnOptionChanged,
            AddNewEntry = OnAddNewMeter,
            RemoveEntry = OnRemoveMeter,
        };
        _selectionListNode.AttachNode(this);

        _configNode = new MeterConfigurationNode
        {
            Position = ContentStartPosition + new Vector2(210.0f, 0.0f),
            Size = ContentSize - new Vector2(210.0f, 0.0f),
            IsVisible = false,
        };
        _configNode.AttachNode(this);

        _nothingSelectedTextNode = new TextNode
        {
            Position = _configNode.Position,
            Size = _configNode.Size,
            AlignmentType = AlignmentType.Center,
            String = "Select a meter or create a new one.",
        };
        _nothingSelectedTextNode.AttachNode(this);
    }

    private void OnOptionChanged(MeterWrapper? newOption)
    {
        if (_configNode == null) return;
        _configNode.IsVisible = newOption != null;
        _nothingSelectedTextNode!.IsVisible = newOption == null;
        _configNode.ConfigurationOption = newOption;
    }

    private void OnAddNewMeter(ModifyListNode<MeterWrapper> listNode)
    {
        var newMeter = new MeterSettings { Name = $"New Meter {System.Config.Meters.Count + 1}" };
        System.Config.Meters.Add(newMeter);

        var wrapper = new MeterWrapper(newMeter);
        _meterWrappers.Add(wrapper);
        listNode.AddOption(wrapper);

        System.OverlayManager.Setup(); // Refresh active overlays
    }

    private void OnRemoveMeter(MeterWrapper wrapper)
    {
        System.Config.Meters.Remove(wrapper.MeterSettings);
        _meterWrappers.Remove(wrapper);
        _selectionListNode?.UpdateList();
        System.OverlayManager.Setup(); // Refresh active overlays
    }
}
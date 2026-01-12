using System.Collections.Generic;
using System.Linq;
using System.Numerics;
using FFXIVClientStructs.FFXIV.Component.GUI;
using KamiToolKit.Nodes;
using KamiToolKit.Premade.Nodes;
using NativeMeters.Configuration;
using NativeMeters.Addons; // For MeterWrapper

namespace NativeMeters.Nodes.Configuration.Meter;

public class MeterManagementNode : SimpleComponentNode
{
    private ModifyListNode<MeterWrapper>? _selectionListNode;
    private MeterConfigurationNode? _configNode;
    private TextNode? _nothingSelectedTextNode;
    private List<MeterWrapper> _meterWrappers = new();

    public MeterManagementNode()
    {
        _meterWrappers = System.Config.Meters.Select(m => new MeterWrapper(m)).ToList();

        _selectionListNode = new ModifyListNode<MeterWrapper>
        {
            Position = Vector2.Zero,
            Size = new Vector2(200.0f, 400.0f),
            SelectionOptions = _meterWrappers,
            OnOptionChanged = OnOptionChanged,
            AddNewEntry = OnAddNewMeter,
            RemoveEntry = OnRemoveMeter,
        };
        _selectionListNode.AttachNode(this);

        _configNode = new MeterConfigurationNode
        {
            Position = new Vector2(210.0f, 0.0f),
            IsVisible = false,
        };
        _configNode.AttachNode(this);

        _nothingSelectedTextNode = new TextNode
        {
            Position = _configNode.Position,
            AlignmentType = AlignmentType.Center,
            String = "Select a meter or create a new one.",
        };
        _nothingSelectedTextNode.AttachNode(this);
    }

    protected override void OnSizeChanged()
    {
        base.OnSizeChanged();
        if (_selectionListNode != null) _selectionListNode.Height = Height;
        if (_configNode != null)
        {
            _configNode.Size = Size - new Vector2(210.0f, 0.0f);
            _nothingSelectedTextNode!.Size = _configNode.Size;
        }
    }

    private void OnOptionChanged(MeterWrapper? newOption)
    {
        _configNode!.IsVisible = newOption != null;
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
        System.OverlayManager.Setup();
    }

    private void OnRemoveMeter(MeterWrapper wrapper)
    {
        System.Config.Meters.Remove(wrapper.MeterSettings);
        _meterWrappers.Remove(wrapper);
        _selectionListNode?.UpdateList();
        System.OverlayManager.Setup();
        if (ReferenceEquals(_configNode?.ConfigurationOption, wrapper)) OnOptionChanged(null);
    }
}
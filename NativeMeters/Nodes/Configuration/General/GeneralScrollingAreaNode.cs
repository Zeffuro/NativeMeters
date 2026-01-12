using System.Numerics;
using KamiToolKit.Nodes;
using NativeMeters.Addons;
using NativeMeters.Configuration;

namespace NativeMeters.Nodes.Configuration.General;

public sealed class GeneralScrollingAreaNode : ScrollingListNode
{
    private _AddonMeterConfigurationWindow? _meterConfigurationAddon;
    private readonly TextButtonNode _meterConfigurationButtonNode;

    public GeneralScrollingAreaNode()
    {
        InitializeMeterConfigurationAddon();
        GeneralSettings config = System.Config.General;

        new ImportExportResetNode().AttachNode(this);

        ItemSpacing = 10;

        AddNode(new GeneralConfigurationNode());

        _meterConfigurationButtonNode = new TextButtonNode
        {
            Size = new Vector2(300, 28),
            String = "Configure Meters",
            OnClick = () => _meterConfigurationAddon?.Toggle(),
        };
        AddNode(_meterConfigurationButtonNode);

        AddNode(new CheckboxNode
        {
            Size = new Vector2(300, 20),
            IsVisible = true,
            String = "Debug Mode",
            IsChecked = config.DebugEnabled,
            OnClick = isChecked =>
            {
                config.DebugEnabled = isChecked;
            }
        });
    }

    private void InitializeMeterConfigurationAddon() {
        if (_meterConfigurationAddon is not null) return;

        _meterConfigurationAddon = new _AddonMeterConfigurationWindow {
            Size = new Vector2(700.0f, 500.0f),
            InternalName = "NativeMeters_MeterConfig",
            Title = "Meter Configuration Window",
        };
    }
}
using KamiToolKit.Premade.GenericListItemNodes;
using NativeMeters.Addons;

namespace NativeMeters.Nodes.Configuration.Meter;

public class MeterListItemNode : GenericListItemNode<MeterWrapper>
{
    protected override uint GetIconId(MeterWrapper data) => data.GetIconId() ?? 0;

    protected override string GetLabelText(MeterWrapper data) => data.GetLabel();

    protected override string GetSubLabelText(MeterWrapper data) => data.GetSubLabel();

    protected override uint? GetId(MeterWrapper data) => data.GetId();

}
using System;
using System.Linq;
using FFXIVClientStructs.FFXIV.Component.GUI;
using KamiToolKit.Components.Search;
using Lumina.Text.ReadOnly;
using NativeMeters.Services;

namespace NativeMeters.Nodes.Configuration.Meter.Search;

public class IconSearchAddon : AbstractSearchAddon<uint, IconListItemNode>
{
    private bool initialized = false;

    public Action<uint>? SelectionResult { get; set; }

    protected override unsafe void OnSetup(AtkUnitBase* addon, Span<AtkValue> atkValueSpan)
    {
        base.OnSetup(addon, atkValueSpan);

        IconRegistry.Initialize(Service.DataManager);
        TryPopulateIcons();
    }

    protected override unsafe void OnUpdate(AtkUnitBase* addon)
    {
        base.OnUpdate(addon);
        TryPopulateIcons();
    }

    protected override void OnSearchInputReceived(ReadOnlySeString searchString)
    {
        ResultsListNode?.OptionsList = OptionsList
            .Where(iconId => IsMatch(iconId, searchString.ToString()))
            .ToList();

        base.OnSearchInputReceived(searchString);
    }

    protected override void OnConfirmClicked()
    {
        if (ResultsListNode?.SelectedItems is { Count: > 0 } selectedItems)
        {
            SelectionResult?.Invoke(selectedItems[0]);
        }

        base.OnConfirmClicked();
    }

    private void TryPopulateIcons()
    {
        if (initialized || !IconRegistry.IsLoaded) return;

        OptionsList = IconRegistry.ValidIconIds.OrderBy(iconId => iconId).ToList();
        initialized = true;
    }

    private static bool IsMatch(uint item, string searchString)
    {
        if (string.IsNullOrEmpty(searchString)) return true;
        return item.ToString().Contains(searchString, StringComparison.OrdinalIgnoreCase);
    }
}

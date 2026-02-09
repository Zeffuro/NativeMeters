using System;
using System.Collections.Generic;
using System.Linq;
using KamiToolKit.Premade.SearchAddons;
using Dalamud.Interface.Internal;
using FFXIVClientStructs.FFXIV.Component.GUI;
using NativeMeters.Services;

namespace NativeMeters.Nodes.Configuration.Meter;

public class IconSearchAddon : BaseSearchAddon<uint, IconListItemNode>
{
    private bool initialized = false;

    public IconSearchAddon()
    {
        SortingOptions = ["Id"];
        ItemSpacing = 2.0f;
    }

    protected override unsafe void OnSetup(AtkUnitBase* addon)
    {
        base.OnSetup(addon);

        IconRegistry.Initialize(Service.DataManager);
    }

    protected override unsafe void OnUpdate(AtkUnitBase* addon)
    {
        base.OnUpdate(addon);

        if (!initialized && IconRegistry.IsLoaded)
        {
            SearchOptions = IconRegistry.ValidIconIds;
            initialized = true;

            SearchOptions = IconRegistry.ValidIconIds;
        }
    }

    protected override int Comparer(uint left, uint right, string sortingString, bool reversed)
    {
        return reversed ? right.CompareTo(left) : left.CompareTo(right);
    }

    protected override bool IsMatch(uint item, string searchString)
    {
        if (string.IsNullOrEmpty(searchString)) return true;
        return item.ToString().Contains(searchString, StringComparison.OrdinalIgnoreCase);
    }
}

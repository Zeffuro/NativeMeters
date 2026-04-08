using System;
using FFXIVClientStructs.FFXIV.Component.GUI;
using KamiToolKit.Premade.Addon.Search;
using NativeMeters.Tags;

namespace NativeMeters.Nodes.Configuration.Meter.Tags;

public class TagSearchAddon : BaseSearchAddon<TagInfo, TagListItemNode>
{
    public Action<string>? OnInsertClicked;

    public TagSearchAddon()
    {
        SortingOptions = ["Category", "Name"];
        ItemSpacing = 2.0f;
    }

    protected override unsafe void OnSetup(AtkUnitBase* addon)
    {
        base.OnSetup(addon);
        SearchOptions = TagRegistry.GetAllTags();
    }

    protected override int Comparer(TagInfo left, TagInfo right, string sortingString, bool reversed)
    {
        int result = sortingString switch
        {
            "Category" => string.Compare(left.Category, right.Category, StringComparison.OrdinalIgnoreCase),
            _ => string.Compare(left.Name, right.Name, StringComparison.OrdinalIgnoreCase)
        };

        if (result == 0)
            result = string.Compare(left.Name, right.Name, StringComparison.OrdinalIgnoreCase);

        return reversed ? -result : result;
    }

    protected override bool IsMatch(TagInfo item, string searchString)
    {
        if (string.IsNullOrEmpty(searchString)) return true;
        return item.Tag.Contains(searchString, StringComparison.OrdinalIgnoreCase) ||
               item.Name.Contains(searchString, StringComparison.OrdinalIgnoreCase) ||
               item.Description.Contains(searchString, StringComparison.OrdinalIgnoreCase);
    }
}

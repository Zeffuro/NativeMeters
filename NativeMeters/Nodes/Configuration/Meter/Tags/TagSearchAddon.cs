using System;
using System.Collections.Generic;
using System.Linq;
using FFXIVClientStructs.FFXIV.Component.GUI;
using KamiToolKit.Components.Search;
using Lumina.Text.ReadOnly;
using NativeMeters.Tags;

namespace NativeMeters.Nodes.Configuration.Meter.Tags;

public enum TagSortOption
{
    Category,
    Name,
}

public class TagSearchAddon : AbstractSearchAddon<TagInfo, TagListItemNode>
{
    public Action<string>? OnInsertClicked;
    public Action<TagInfo>? SelectionResult { get; set; }

    protected override unsafe void OnSetup(AtkUnitBase* addon, Span<AtkValue> atkValueSpan)
    {
        base.OnSetup(addon, atkValueSpan);
        OptionsList = SortTags(TagRegistry.GetAllTags());
    }

    protected override void OnSearchInputReceived(ReadOnlySeString searchString)
    {
        ResultsListNode?.OptionsList = OptionsList
            .Where(tagInfo => IsMatch(tagInfo, searchString.ToString()))
            .ToList();

        base.OnSearchInputReceived(searchString);
    }

    protected override void OnConfirmClicked()
    {
        var selectedTag = ResultsListNode?.SelectedItems.FirstOrDefault();
        if (selectedTag is not null)
        {
            SelectionResult?.Invoke(selectedTag);
        }

        base.OnConfirmClicked();
    }

    private static List<TagInfo> SortTags(IEnumerable<TagInfo> tags)
    {
        return tags
            .OrderBy(tagInfo => tagInfo.Category, StringComparer.OrdinalIgnoreCase)
            .ThenBy(tagInfo => tagInfo.Name, StringComparer.OrdinalIgnoreCase)
            .ToList();
    }

    private static bool IsMatch(TagInfo item, string searchString)
    {
        if (string.IsNullOrEmpty(searchString)) return true;
        return item.Tag.Contains(searchString, StringComparison.OrdinalIgnoreCase) ||
               item.Name.Contains(searchString, StringComparison.OrdinalIgnoreCase) ||
               item.Description.Contains(searchString, StringComparison.OrdinalIgnoreCase);
    }
}

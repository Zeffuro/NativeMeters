using System;
using System.Numerics;
using FFXIVClientStructs.FFXIV.Component.GUI;
using KamiToolKit.Nodes;
using NativeMeters.Configuration;
using NativeMeters.Nodes.Configuration.Dtr;
using CheckboxNode = NativeMeters.Nodes.Input.SyncedCheckboxNode;

namespace NativeMeters.Nodes.Configuration.General;

public sealed class GeneralScrollingAreaNode : ScrollingNode<VerticalListNode>
{
    private const int FirstContentNavIndex = 6;
    private const float ActionBarHeight = 36.0f;

    private readonly ImportExportResetNode actionBar;
    private readonly CategoryTextNode heading;

    public int TabBarNavIndex { get; set; } = 1;

    public GeneralScrollingAreaNode()
    {
        GeneralSettings config = System.Config.General;

        actionBar = new ImportExportResetNode { Height = ActionBarHeight };
        actionBar.AttachNode(this);
        heading = new CategoryTextNode
        {
            TextFlags = TextFlags.Ellipsis,
            String = "General Configuration",
            Height = 18,
        };
        heading.Y = (ActionBarHeight - heading.Height) / 2.0f;
        heading.AttachNode(this);

        ContentNode.ItemSpacing = 10;
        ContentNode.FitContents = true;

        ContentNode.AddNode(
        [new GeneralConfigurationNode(),
            new DtrConfigurationNode(),
            new CheckboxNode {
                Size = new Vector2(300, 20),
                IsVisible = true,
                String = "Debug Mode",
                IsChecked = config.DebugEnabled,
                OnClick = isChecked =>
                {
                    config.DebugEnabled = isChecked;
                }
            }
        ]);

        RecalculateConfigurationLayout();
    }

    protected override void OnSizeChanged()
    {
        var previousScrollPosition = ScrollBarNode.ScrollPosition;
        ContentNode.FitWidth = true;
        base.OnSizeChanged();

        ConfigurationLayout.SetWidth(actionBar, Width);
        ConfigurationLayout.SetWidth(heading, Math.Max(0, actionBar.AreaRemaining - 4.0f));
        var viewportTop = Math.Min(ActionBarHeight, Height);
        var viewportHeight = Height - viewportTop;
        ClippingContentNode.Y = viewportTop;
        ClippingContentNode.Height = viewportHeight;
        ScrollingCollisionNode.Height = viewportHeight;
        ScrollBarNode.Y = viewportTop;
        ScrollBarNode.Height = viewportHeight;
        ScrollBarNode.UpdateScrollParams();
        ScrollBarNode.ScrollPosition = Math.Clamp(previousScrollPosition, 0, ScrollBarNode.ScrollMaxPosition);
        RecalculateConfigurationLayout();
    }

    private void RecalculateConfigurationLayout()
    {
        ConfigurationNavigation.Apply(ContentNode, FirstContentNavIndex, TabBarNavIndex, TabBarNavIndex);
    }
}

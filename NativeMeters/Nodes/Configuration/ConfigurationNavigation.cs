using System;
using System.Collections.Generic;
using System.Linq;
using KamiToolKit.BaseTypes;
using KamiToolKit.BaseTypes.ComponentNode;
using KamiToolKit.Interfaces;
using NativeMeters.Nodes.LayoutNodes;

namespace NativeMeters.Nodes.Configuration;

public interface IConfigurationNavigationNode
{
    IEnumerable<ConfigurationNavigationTarget> GetNavigationTargets();
}

public readonly struct ConfigurationNavigationTarget
{
    private readonly Action<int> setNavIndex;
    private readonly Action<int> setNavUp;
    private readonly Action<int> setNavDown;
    private readonly Action<int> setNavLeft;
    private readonly Action<int> setNavRight;

    public NodeBase Node { get; }

    private ConfigurationNavigationTarget(
        NodeBase node,
        Action<int> setNavIndex,
        Action<int> setNavUp,
        Action<int> setNavDown,
        Action<int> setNavLeft,
        Action<int> setNavRight)
    {
        Node = node;
        this.setNavIndex = setNavIndex;
        this.setNavUp = setNavUp;
        this.setNavDown = setNavDown;
        this.setNavLeft = setNavLeft;
        this.setNavRight = setNavRight;
    }

    public bool IsVisible => Node.IsVisible;

    public void SetNavigation(int navIndex, int navUp, int navDown, int navLeft, int navRight)
    {
        setNavIndex(navIndex);
        setNavUp(navUp);
        setNavDown(navDown);
        setNavLeft(navLeft);
        setNavRight(navRight);
    }

    public static ConfigurationNavigationTarget From(ComponentNode node)
        => new(
            node,
            value => node.NavIndex = value,
            value => node.NavUp = value,
            value => node.NavDown = value,
            value => node.NavLeft = value,
            value => node.NavRight = value);

    public static ConfigurationNavigationTarget From(NodeBase node, IControllerNavigable navigable)
        => new(
            node,
            value => navigable.NavIndex = value,
            value => navigable.NavUp = value,
            value => navigable.NavDown = value,
            value => navigable.NavLeft = value,
            value => navigable.NavRight = value);
}

internal static class ConfigurationNavigation
{
    public static int Apply(NodeBase root, int startIndex, int navUp, int navDown, int navLeft = 0, int navRight = 0)
    {
        var targets = GetNavigationTargets(root).Where(target => target.IsVisible).ToList();
        if (targets.Count == 0) return startIndex;

        for (var index = 0; index < targets.Count; index++)
        {
            var navIndex = startIndex + index;
            var up = index == 0 ? navUp : navIndex - 1;
            var down = index == targets.Count - 1 ? navDown : navIndex + 1;

            targets[index].SetNavigation(navIndex, up, down, navLeft, navRight);
        }

        return startIndex + targets.Count;
    }

    public static IReadOnlyList<ConfigurationNavigationTarget> GetNavigationTargets(NodeBase root)
        => EnumerateNavigationTargets(root).ToList();

    private static IEnumerable<ConfigurationNavigationTarget> EnumerateNavigationTargets(NodeBase node)
    {
        if (!node.IsVisible) yield break;

        if (node is IConfigurationNavigationNode navigationNode)
        {
            foreach (var target in navigationNode.GetNavigationTargets())
            {
                if (target.IsVisible) yield return target;
            }

            yield break;
        }

        if (node is ComponentNode componentNode)
        {
            yield return ConfigurationNavigationTarget.From(componentNode);
            yield break;
        }

        if (node is IControllerNavigable navigable)
        {
            yield return ConfigurationNavigationTarget.From(node, navigable);
            yield break;
        }

        if (node is ILayoutListNode layoutNode)
        {
            foreach (var childNode in layoutNode.Nodes)
            {
                foreach (var target in EnumerateNavigationTargets(childNode))
                {
                    yield return target;
                }
            }
        }
    }
}

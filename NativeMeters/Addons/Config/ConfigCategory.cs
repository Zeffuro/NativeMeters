using System.Collections.Generic;
using System.Linq;
using System.Numerics;
using KamiToolKit.Nodes;
using NativeMeters.Addons.Config.EntryTypes;
using NativeMeters.Classes;
using NativeMeters.Extensions;

namespace NativeMeters.Addons.Config;

public class ConfigCategory {
    public required string CategoryLabel { get; init; }
    public required ISavable ConfigObject { get; init; }

    private readonly List<IConfigEntry> configEntries = [];

    public TabbedVerticalListNode BuildNode() {
        var tabbedListNode = new TabbedVerticalListNode {
            FitWidth = true,
            IsVisible = true,
        };

        tabbedListNode.AddNode(new ResNode {
            Size = new Vector2(4.0f, 4.0f),
            IsVisible = true,
        });

        tabbedListNode.AddNode(new SimpleLabelNode {
            String = CategoryLabel,
            IsVisible = true,
        });

        tabbedListNode.AddTab(1);

        foreach (var entry in configEntries) {
            if (entry is IndentEntry) {
                tabbedListNode.AddTab(1);
                continue;
            }

            tabbedListNode.AddNode(entry.BuildNode());
        }

        tabbedListNode.SubtractTab(1);

        return tabbedListNode;
    }

    public ConfigCategory AddCheckbox(string label, string memberName) {
        var memberInfo = ConfigObject.GetType().GetMember(memberName).FirstOrDefault();
        if (memberInfo is null) return this;

        var initialValue = memberInfo.GetValue<bool>(ConfigObject);

        configEntries.Add(new CheckBoxConfig {
            Label = label,
            MemberInfo = memberInfo,
            Config = ConfigObject,
            InitialState = initialValue,
        });

        return this;
    }

    public ConfigCategory AddIntSlider(string label, int min, int max, string memberName) {
        var memberInfo = ConfigObject.GetType().GetMember(memberName).FirstOrDefault();
        if (memberInfo is null) return this;

        var initialValue = memberInfo.GetValue<int>(ConfigObject);

        configEntries.Add(new IntSliderConfig {
            Label = label,
            MemberInfo = memberInfo,
            Config = ConfigObject,
            Range = min..max,
            InitialValue = initialValue,
        });

        return this;
    }

    public ConfigCategory AddFloatSlider(string label, float min, float max, int decimalPlaces, float speed, string memberName) {
        var memberInfo = ConfigObject.GetType().GetMember(memberName).FirstOrDefault();
        if (memberInfo is null) return this;

        var initialValue = memberInfo.GetValue<float>(ConfigObject);

        configEntries.Add(new FloatSliderConfig {
            Label = label,
            MemberInfo = memberInfo,
            Config = ConfigObject,
            DecimalPlaces = decimalPlaces,
            MaxValue = max,
            MinValue = min,
            InitialValue = initialValue,
            StepSpeed = speed,
        });

        return this;
    }

    public ConfigCategory AddIndent() {
        configEntries.Add(new IndentEntry());
        return this;
    }
}

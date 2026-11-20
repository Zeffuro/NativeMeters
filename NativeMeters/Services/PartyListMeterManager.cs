using System;
using System.Collections.Generic;
using System.Linq;
using System.Numerics;
using Dalamud.Utility;
using FFXIVClientStructs.FFXIV.Client.UI;
using FFXIVClientStructs.FFXIV.Component.GUI;
using KamiToolKit.Classes;
using KamiToolKit.Controllers;
using KamiToolKit.Enums;
using KamiToolKit.Extensions;
using KamiToolKit.Nodes;
using NativeMeters.Configuration;
using NativeMeters.Data.Stats;
using NativeMeters.Extensions;
using NativeMeters.Models;
using NativeMeters.Tags;
using NativeMeters.Utilities;

namespace NativeMeters.Services;

public sealed unsafe class PartyListMeterManager : IDisposable
{
    private const int MaxHudMemberCount = 10;
    private const float MinimumScale = 0.001f;

    private readonly TextNode?[] memberTextNodes = new TextNode?[MaxHudMemberCount];
    private readonly PartyListMemberBars memberBars = new(MaxHudMemberCount);
    private readonly string?[] cachedText = new string?[MaxHudMemberCount];

    private AddonController<AddonPartyList>? partyListController;
    private ResNode? rootNode;
    private TextNode? raidTextNode;
    private string? cachedRaidText;
    private nint attachedAddon;
    private bool isEnabled;
    private bool hasLoggedUpdateException;

    private PartyListMeterSettings Settings => System.Config.PartyListMeter;

    public void UpdateSettings()
    {
        if (Service.Framework.IsFrameworkUnloading)
            return;

        if (!ThreadSafety.IsMainThread)
        {
            try
            {
                Service.Framework.RunOnFrameworkThread(UpdateSettingsOnFrameworkThread);
            }
            catch (Exception exception)
            {
                Service.Logger.Error(exception, "Failed to schedule party list meter settings update.");
            }

            return;
        }

        UpdateSettingsOnFrameworkThread();
    }

    public void Dispose()
    {
        if (!ThreadSafety.IsMainThread)
            return;

        Disable();
    }

    private void UpdateSettingsOnFrameworkThread()
    {
        if (ShouldBeEnabled())
        {
            Enable();
        }
        else
        {
            Disable();
        }
    }

    private static bool ShouldBeEnabled()
        => Service.ClientState.IsLoggedIn
           && System.Config.General.IsEnabled
           && System.Config.PartyListMeter.Enabled;

    private void Enable()
    {
        if (isEnabled)
            return;

        partyListController = new AddonController<AddonPartyList>
        {
            AddonName = "_PartyList",
            OnSetup = SetupPartyList,
            OnUpdate = UpdatePartyList,
            OnFinalize = _ => ClearAttachedNodeReferences(),
        };

        partyListController.Enable();
        isEnabled = true;
    }

    private void Disable()
    {
        if (!isEnabled && partyListController == null)
            return;

        DisposeAttachedNodes();
        partyListController?.Dispose();
        partyListController = null;
        isEnabled = false;
        hasLoggedUpdateException = false;
    }

    private void SetupPartyList(AddonPartyList* addon)
    {
        try
        {
            EnsureAttachedNodes(addon);
        }
        catch (Exception exception)
        {
            Service.Logger.Error(exception, "Failed to setup party list meter nodes.");
            ClearAttachedNodeReferences();
        }
    }

    private void UpdatePartyList(AddonPartyList* addon)
    {
        try
        {
            UpdatePartyListUnsafe(addon);
            hasLoggedUpdateException = false;
        }
        catch (Exception exception)
        {
            HideAllNodes();

            if (hasLoggedUpdateException)
                return;

            hasLoggedUpdateException = true;
            Service.Logger.Error(exception, "Failed to update party list meter.");
        }
    }

    private void UpdatePartyListUnsafe(AddonPartyList* addon)
    {
        if (!ShouldRender() || !IsPartyListUsable(addon))
        {
            HideAllNodes();
            return;
        }

        EnsureAttachedNodes(addon);

        var combatants = System.ActiveMeterService.GetCombatants().ToList();
        if (combatants.Count == 0)
        {
            HideAllNodes();
            return;
        }

        var activeSlots = new bool[MaxHudMemberCount];
        var hudMembers = PartyListUtilities.GetHudMembers(addon, Settings.Anchor);
        var resolvedCombatants = new Dictionary<int, Combatant>();

        foreach (var hudData in hudMembers)
        {
            if (hudData.HudIndex is < 0 or >= MaxHudMemberCount)
                continue;

            if (!ShouldShowMember(hudData) || !IsAnchorUsable(hudData.AnchorNode))
                continue;

            var combatant = ResolveCombatant(hudData, combatants);
            if (combatant != null)
                resolvedCombatants[hudData.HudIndex] = combatant;
        }

        var fillSelector = Settings.ShowMemberBars
            ? StatSelector.GetStatSelector(StatSelector.NormalizeStatSelector(Settings.BarFillStat))
            : null;
        var maxFill = fillSelector == null || resolvedCombatants.Count == 0
            ? 0.0
            : resolvedCombatants.Values.Max(fillSelector);
        var topMemberHudIndex = ResolveTopMemberHudIndex(resolvedCombatants);

        if (fillSelector == null)
            memberBars.Retire();

        foreach (var hudData in hudMembers)
        {
            if (hudData.HudIndex is < 0 or >= MaxHudMemberCount)
                continue;

            activeSlots[hudData.HudIndex] = true;

            if (!ShouldShowMember(hudData) || !IsAnchorUsable(hudData.AnchorNode))
            {
                HideMemberSlot(hudData.HudIndex);
                continue;
            }

            if (!resolvedCombatants.TryGetValue(hudData.HudIndex, out var combatant))
            {
                HideMemberSlot(hudData.HudIndex);
                continue;
            }

            if (fillSelector != null && rootNode != null)
            {
                var progress = maxFill > 0.0 ? fillSelector(combatant) / maxFill : 0.0;
                var anchorPosition = GetLocalPosition(addon, hudData.AnchorNode);
                memberBars.Update(rootNode, hudData.HudIndex, Settings, combatant, anchorPosition, (float)progress);
            }

            ApplyTextNodeSettings(hudData.HudIndex, hudData.AnchorNode, addon, combatant, hudData.HudIndex == topMemberHudIndex);
        }

        HideInactiveSlots(activeSlots);
        ApplyRaidTextNodeSettings(addon);
    }

    private void EnsureAttachedNodes(AddonPartyList* addon)
    {
        if (addon == null || ((AtkUnitBase*)addon)->RootNode == null)
            return;

        var addonPointer = (nint)addon;
        if (attachedAddon != 0 && attachedAddon != addonPointer)
            ClearAttachedNodeReferences();

        if (rootNode != null && attachedAddon == addonPointer)
            return;

        try
        {
            rootNode = new ResNode
            {
                Position = Vector2.Zero,
                Size = ((AtkUnitBase*)addon)->RootSize,
                IsVisible = true,
            };
            rootNode.RemoveNodeFlags(NodeFlags.EmitsEvents, NodeFlags.RespondToMouse, NodeFlags.HasCollision);

            var barStyle = Settings.MemberBarStyle;
            for (var index = 0; index < memberTextNodes.Length; index++)
            {
                if (Settings.ShowMemberBars)
                    memberBars.Attach(rootNode, index, barStyle);

                var textNode = CreateTextNode();
                textNode.AttachNode(rootNode);
                memberTextNodes[index] = textNode;
            }

            raidTextNode = CreateTextNode();
            raidTextNode.AttachNode(rootNode);

            rootNode.AttachNode(((AtkUnitBase*)addon)->RootNode, NodePosition.AsLastChild);
            attachedAddon = addonPointer;
        }
        catch
        {
            DisposeAttachedNodes();
            throw;
        }
    }

    private static TextNode CreateTextNode()
    {
        var textNode = new TextNode
        {
            IsVisible = false,
        };

        textNode.RemoveNodeFlags(NodeFlags.EmitsEvents, NodeFlags.RespondToMouse, NodeFlags.HasCollision);

        return textNode;
    }

    private bool ShouldRender()
    {
        if (!ShouldBeEnabled())
            return false;

        if (Settings.HideWhenNoCombatData
            && !System.Config.General.PreviewEnabled
            && !System.ActiveMeterService.HasCombatData())
        {
            return false;
        }

        return true;
    }

    private static bool IsPartyListUsable(AddonPartyList* addon)
    {
        if (addon == null)
            return false;

        var unitBase = (AtkUnitBase*)addon;
        return unitBase->IsReady
               && unitBase->IsVisible
               && unitBase->RootNode != null
               && unitBase->RootNode->IsVisible();
    }

    private static bool IsAnchorUsable(AtkResNode* anchorNode)
        => anchorNode != null && anchorNode->IsActuallyVisible;

    private bool ShouldShowMember(PartyListHudData hudData)
        => hudData.IsSelf ? Settings.ShowSelf : Settings.ShowPartyMembers;

    private Combatant? ResolveCombatant(PartyListHudData hudData, IReadOnlyList<Combatant> combatants)
    {
        if (hudData.IsSelf)
        {
            var self = combatants.FirstOrDefault(combatant => combatant.IsSelf);
            if (self != null)
                return self;
        }

        var memberName = hudData.Name;
        if (!string.IsNullOrWhiteSpace(memberName))
        {
            var namedCombatant = combatants.FirstOrDefault(combatant =>
                combatant.Name.Equals(memberName, StringComparison.OrdinalIgnoreCase));

            if (namedCombatant != null)
                return namedCombatant;
        }

        return System.Config.General.PreviewEnabled && hudData.HudIndex < combatants.Count
            ? combatants[hudData.HudIndex]
            : null;
    }

    private int? ResolveTopMemberHudIndex(IReadOnlyDictionary<int, Combatant> resolvedCombatants)
    {
        if (!Settings.HighlightTopMember || resolvedCombatants.Count == 0)
            return null;

        var statName = StatSelector.NormalizeStatSelector(Settings.TopMemberStat);
        var selector = StatSelector.GetStatSelector(statName);

        var topValue = 0.0;
        int? topHudIndex = null;

        foreach (var (hudIndex, combatant) in resolvedCombatants)
        {
            var value = selector(combatant);
            if (value <= topValue)
                continue;

            topValue = value;
            topHudIndex = hudIndex;
        }

        return topHudIndex;
    }

    private void ApplyTextNodeSettings(int slotIndex, AtkResNode* anchorNode, AddonPartyList* addon, Combatant combatant, bool isTopMember)
    {
        var textNode = memberTextNodes[slotIndex];
        if (textNode == null)
            return;

        var unitBase = (AtkUnitBase*)addon;
        var localAnchorPosition = GetLocalPosition(addon, anchorNode);
        var text = TagEngine.ProcessCached(isTopMember ? Settings.TopMemberFormat : Settings.MemberFormat, combatant, textNode);

        if (rootNode != null)
            rootNode.Size = unitBase->RootSize;

        textNode.Position = localAnchorPosition + new Vector2(Settings.OffsetX, Settings.OffsetY);
        var textSize = new Vector2(Settings.Width, Settings.Height);
        if (textNode.Size != textSize) textNode.Size = textSize;
        ApplyMemberTextStyle(textNode, isTopMember ? Settings.TopMemberTextColor : Settings.TextColor);

        if (cachedText[slotIndex] != text)
        {
            textNode.String = text;
            cachedText[slotIndex] = text;
        }

        textNode.IsVisible = true;
        textNode.MarkDirty();
    }

    private void ApplyRaidTextNodeSettings(AddonPartyList* addon)
    {
        var textNode = raidTextNode;
        if (textNode == null || !Settings.ShowRaidDps)
        {
            HideRaidTextNode();
            return;
        }

        var encounter = System.ActiveMeterService.GetEncounter();
        if (encounter == null)
        {
            HideRaidTextNode();
            return;
        }

        var anchorNode = addon->PartyTypeTextNode != null
            ? (AtkResNode*)addon->PartyTypeTextNode
            : ((AtkUnitBase*)addon)->RootNode;

        if (!IsAnchorUsable(anchorNode))
        {
            HideRaidTextNode();
            return;
        }

        var localAnchorPosition = GetLocalPosition(addon, anchorNode);
        var text = TagEngine.ProcessCached(Settings.RaidDpsFormat, encounter, textNode);

        textNode.Position = localAnchorPosition + new Vector2(Settings.RaidOffsetX, Settings.RaidOffsetY);
        var textSize = new Vector2(Settings.RaidWidth, Settings.RaidHeight);
        if (textNode.Size != textSize) textNode.Size = textSize;
        ApplyRaidTextStyle(textNode);

        if (cachedRaidText != text)
        {
            textNode.String = text;
            cachedRaidText = text;
        }

        textNode.IsVisible = true;
        textNode.MarkDirty();
    }

    private void ApplyMemberTextStyle(TextNode textNode, Vector4 textColor)
        => ApplyTextStyle(
            textNode,
            Settings.FontSize,
            Settings.FontType,
            Settings.TextFlags,
            Settings.MemberAlignment,
            textColor,
            Settings.TextOutlineColor);

    private void ApplyRaidTextStyle(TextNode textNode)
        => ApplyTextStyle(
            textNode,
            Settings.RaidFontSize,
            Settings.RaidFontType,
            Settings.RaidTextFlags,
            Settings.RaidAlignment,
            Settings.RaidTextColor,
            Settings.RaidTextOutlineColor);

    private static void ApplyTextStyle(
        TextNode textNode,
        uint fontSize,
        FontType fontType,
        TextFlags textFlags,
        AlignmentType alignmentType,
        Vector4 textColor,
        Vector4 textOutlineColor)
    {
        if (textNode.FontSize != fontSize) textNode.FontSize = fontSize;
        if (textNode.FontType != fontType) textNode.FontType = fontType;
        if (textNode.TextFlags != textFlags) textNode.TextFlags = textFlags;
        if (textNode.AlignmentType != alignmentType) textNode.AlignmentType = alignmentType;
        textNode.TextColor = textColor;
        textNode.TextOutlineColor = textOutlineColor;
    }

    private static Vector2 GetLocalPosition(AddonPartyList* addon, AtkResNode* anchorNode)
    {
        var unitBase = (AtkUnitBase*)addon;
        var addonScale = GetSafeScale(unitBase->Scale);
        var anchorScreenPosition = new Vector2(anchorNode->ScreenX, anchorNode->ScreenY);
        var addonScreenPosition = unitBase->Position;

        return (anchorScreenPosition - addonScreenPosition) / addonScale;
    }

    private static float GetSafeScale(float scale)
        => Math.Abs(scale) > MinimumScale ? scale : 1.0f;

    private void HideInactiveSlots(bool[] activeSlots)
    {
        for (var index = 0; index < memberTextNodes.Length; index++)
        {
            if (!activeSlots[index])
                HideMemberSlot(index);
        }
    }

    private void HideAllNodes()
    {
        for (var index = 0; index < memberTextNodes.Length; index++)
        {
            HideMemberSlot(index);
        }

        HideRaidTextNode();
    }

    private void HideMemberTextNode(int index)
    {
        if (index < 0 || index >= memberTextNodes.Length)
            return;

        if (memberTextNodes[index] is { } textNode)
            textNode.IsVisible = false;

        cachedText[index] = null;
    }

    private void HideMemberSlot(int index)
    {
        HideMemberTextNode(index);
        memberBars.Hide(index);
    }

    private void HideRaidTextNode()
    {
        if (raidTextNode is { } textNode)
            textNode.IsVisible = false;

        cachedRaidText = null;
    }

    private void DisposeAttachedNodes()
    {
        rootNode.DisposeLater();
        ClearAttachedNodeReferences();
    }

    private void ClearAttachedNodeReferences()
    {
        rootNode = null;
        raidTextNode = null;
        cachedRaidText = null;
        attachedAddon = 0;
        memberBars.ClearReferences();

        for (var index = 0; index < memberTextNodes.Length; index++)
        {
            memberTextNodes[index] = null;
            cachedText[index] = null;
        }
    }
}

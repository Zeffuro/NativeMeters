using System.Collections.Generic;
using FFXIVClientStructs.FFXIV.Client.UI;
using FFXIVClientStructs.FFXIV.Client.UI.Agent;
using FFXIVClientStructs.FFXIV.Component.GUI;
using NativeMeters.Configuration;
using NativeMeters.Services;

namespace NativeMeters.Utilities;

public static unsafe class PartyListUtilities
{
    private const uint EmptyEntityId = 0xE0000000;

    public static List<PartyListHudData> GetHudMembers(AddonPartyList* addon, PartyListMeterAnchor anchor)
    {
        var hudMembers = new List<PartyListHudData>();

        if (addon == null)
            return hudMembers;

        var agentHud = AgentHUD.Instance();
        if (agentHud == null)
            return hudMembers;

        var memberCount = 0;
        foreach (ref var member in agentHud->PartyMembers)
        {
            if (member.EntityId is not EmptyEntityId)
                memberCount++;
        }

        for (var hudIndex = 0; hudIndex < memberCount; hudIndex++)
        {
            ref var hudMember = ref agentHud->PartyMembers[hudIndex];
            if (hudMember.Object == null)
                continue;

            AtkResNode* anchorNode;

            if (hudMember.ContentId is 0)
            {
                var trustIndex = hudIndex - 1;
                if (trustIndex < 0 || trustIndex >= addon->TrustMembers.Length)
                    continue;

                anchorNode = GetAnchorNode(ref addon->TrustMembers[trustIndex], anchor);
            }
            else
            {
                if (hudMember.Index >= addon->PartyMembers.Length)
                    continue;

                anchorNode = GetAnchorNode(ref addon->PartyMembers[hudMember.Index], anchor);
            }

            if (anchorNode == null)
                continue;

            var isSelf = false;
            if (Service.ObjectTable.LocalPlayer is { EntityId: var playerId })
                isSelf = hudMember.EntityId == playerId;

            hudMembers.Add(new PartyListHudData(hudIndex, hudMember.Name.ToString(), isSelf, anchorNode));
        }

        return hudMembers;
    }

    private static AtkResNode* GetAnchorNode(ref AddonPartyList.PartyListMemberStruct partyListMember, PartyListMeterAnchor anchor)
    {
        var preferredAnchor = anchor switch
        {
            PartyListMeterAnchor.HpBar => GetHpBarAnchor(ref partyListMember),
            PartyListMeterAnchor.MpBar => GetMpBarAnchor(ref partyListMember),
            PartyListMeterAnchor.MemberRow => GetMemberRowAnchor(ref partyListMember),
            _ => null,
        };

        return preferredAnchor != null
            ? preferredAnchor
            : GetFallbackAnchor(ref partyListMember);
    }

    private static AtkResNode* GetFallbackAnchor(ref AddonPartyList.PartyListMemberStruct partyListMember)
    {
        if (partyListMember.NameAndBarsContainer != null)
            return partyListMember.NameAndBarsContainer;

        var hpBarAnchor = GetHpBarAnchor(ref partyListMember);
        if (hpBarAnchor != null)
            return hpBarAnchor;

        var mpBarAnchor = GetMpBarAnchor(ref partyListMember);
        if (mpBarAnchor != null)
            return mpBarAnchor;

        return GetMemberRowAnchor(ref partyListMember);
    }

    private static AtkResNode* GetHpBarAnchor(ref AddonPartyList.PartyListMemberStruct partyListMember)
    {
        if (partyListMember.HPGaugeBar != null && partyListMember.HPGaugeBar->OwnerNode != null)
            return (AtkResNode*)partyListMember.HPGaugeBar->OwnerNode;

        return partyListMember.HPGaugeComponent != null && partyListMember.HPGaugeComponent->OwnerNode != null
            ? (AtkResNode*)partyListMember.HPGaugeComponent->OwnerNode
            : null;
    }

    private static AtkResNode* GetMpBarAnchor(ref AddonPartyList.PartyListMemberStruct partyListMember)
    {
        if (partyListMember.MPGaugeBar != null && partyListMember.MPGaugeBar->OwnerNode != null)
            return (AtkResNode*)partyListMember.MPGaugeBar->OwnerNode;

        return null;
    }

    private static AtkResNode* GetMemberRowAnchor(ref AddonPartyList.PartyListMemberStruct partyListMember)
    {
        return partyListMember.PartyMemberComponent != null && partyListMember.PartyMemberComponent->OwnerNode != null
            ? (AtkResNode*)partyListMember.PartyMemberComponent->OwnerNode
            : null;
    }
}

public readonly unsafe struct PartyListHudData(
    int hudIndex,
    string name,
    bool isSelf,
    AtkResNode* anchorNode)
{
    public int HudIndex { get; } = hudIndex;
    public string Name { get; } = name;
    public bool IsSelf { get; } = isSelf;
    public AtkResNode* AnchorNode { get; } = anchorNode;
}

using System.Collections.Generic;
using Dalamud.Game.ClientState.Objects.Enums;
using FFXIVClientStructs.FFXIV.Client.Game.Character;

namespace NativeMeters.Services.Internal;

public unsafe class StatusTracker
{
    public List<ulong> GetDoTSources(uint targetId)
    {
        var sources = new List<ulong>();
        var targetObj = Service.ObjectTable.SearchById(targetId);
        if (targetObj == null || targetObj.Address == nint.Zero) return sources;

        var chara = (Character*)targetObj.Address;
        var statusManager = chara->GetStatusManager();
        if (statusManager == null) return sources;

        foreach (ref var status in statusManager->Status)
        {
            if (status.StatusId == 0 || status.SourceObject == 0) continue;

            var sourceObj = Service.ObjectTable.SearchById(status.SourceObject);
            if (sourceObj != null && sourceObj.ObjectKind == ObjectKind.Player)
            {
                if (!sources.Contains(status.SourceObject))
                    sources.Add(status.SourceObject);
            }
        }
        return sources;
    }

    public ulong? GetSource(uint targetId, uint statusId)
    {
        var targetObj = Service.ObjectTable.SearchById(targetId);
        if (targetObj == null || targetObj.Address == nint.Zero) return null;

        var chara = (Character*)targetObj.Address;
        var statusManager = chara->GetStatusManager();
        if (statusManager == null) return null;

        foreach (ref var status in statusManager->Status)
        {
            if (status.StatusId == statusId && status.SourceObject != 0)
                return status.SourceObject;
        }
        return null;
    }

    public void Clear() { }
}

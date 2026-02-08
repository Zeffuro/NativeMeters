using System.Collections.Generic;
using Dalamud.Game.ClientState.Objects.SubKinds;
using FFXIVClientStructs.FFXIV.Client.Game.Character;
using Lumina.Excel.Sheets;

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

        var statusSheet = Service.DataManager.GetExcelSheet<Status>();
        var seenSources = new HashSet<ulong>();

        foreach (ref var status in statusManager->Status)
        {
            if (status.StatusId == 0 || status.SourceObject == 0) continue;

            if (Service.ObjectTable.SearchById(status.SourceObject) is not IPlayerCharacter) continue;

            var statusRow = statusSheet.GetRowOrDefault(status.StatusId);
            if (statusRow == null) continue;

            if (seenSources.Add(status.SourceObject))
            {
                sources.Add(status.SourceObject);
            }
        }

        return sources;
    }

    public ulong? GetSource(uint targetId, uint statusId)
    {
        if (statusId == 0) return null;

        var targetObj = Service.ObjectTable.SearchById(targetId);
        if (targetObj == null || targetObj.Address == nint.Zero) return null;

        var chara = (Character*)targetObj.Address;
        var statusManager = chara->GetStatusManager();
        if (statusManager == null) return null;

        foreach (ref var status in statusManager->Status)
        {
            if (status.StatusId == statusId && status.SourceObject != 0)
            {
                return status.SourceObject;
            }
        }

        return null;
    }

    public void Clear() { }
}

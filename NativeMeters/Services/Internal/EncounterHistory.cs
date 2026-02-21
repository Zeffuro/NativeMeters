using System.Collections.Generic;
using NativeMeters.Models.Breakdown;

namespace NativeMeters.Services.Internal;

public class EncounterHistory
{
    private readonly List<EncounterSnapshot> history = new();
    private const int MaxHistory = 10;

    public void Save(EncounterSnapshot snapshot)
    {
        history.Add(snapshot);
        if (history.Count > MaxHistory)
            history.RemoveAt(0);
    }

    public IReadOnlyList<EncounterSnapshot> Encounters => history;
    public EncounterSnapshot? Latest => history.Count > 0 ? history[^1] : null;
    public int Count => history.Count;
    public void Clear() => history.Clear();
}

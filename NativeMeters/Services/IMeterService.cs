using System;
using NativeMeters.Models;

namespace NativeMeters.Services;

public interface IMeterService
{
    CombatDataMessage? CurrentCombatData { get; }
    event Action? CombatDataUpdated;
}
using System;
using System.Collections.Generic;
using Dalamud.Game.ClientState.Objects.Types;
using NativeMeters.Services;

namespace NativeMeters.Tags;

public static class CustomTagRegistry
{
    private static readonly Dictionary<string, Func<object, object?>> Registry = new(StringComparer.OrdinalIgnoreCase);

    static CustomTagRegistry()
    {
        Register("targethppct", _ =>
        {
            var target = Service.TargetManager.Target;
            if (target is IBattleChara bc && bc.MaxHp > 0)
                return bc.CurrentHp * 100.0 / bc.MaxHp;
            return 0.0;
        });

        Register("targetname", _ =>
        {
            var target = Service.TargetManager.Target;
            return target != null ? target.Name.TextValue : string.Empty;
        });

        Register("focushppct", _ =>
        {
            var target = Service.TargetManager.FocusTarget;
            if (target is IBattleChara bc && bc.MaxHp > 0)
                return bc.CurrentHp * 100.0 / bc.MaxHp;
            return 0.0;
        });

        Register("focusname", _ =>
        {
            var target = Service.TargetManager.FocusTarget;
            return target != null ? target.Name.TextValue : string.Empty;
        });
    }

    public static void Register(string tag, Func<object, object?> resolver)
    {
        Registry[tag] = resolver;
    }

    public static bool TryResolve(string tag, object context, out object? result)
    {
        if (Registry.TryGetValue(tag, out var resolver))
        {
            result = resolver(context);
            return true;
        }

        result = null;
        return false;
    }
}

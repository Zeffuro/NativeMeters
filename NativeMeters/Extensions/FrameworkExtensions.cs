using System;
using System.Threading.Tasks;
using Dalamud.Plugin.Services;
using Dalamud.Utility;

namespace NativeMeters.Extensions;

public static class FrameworkExtensions
{
    public static Task RunOnFrameworkThreadIfNeeded(this IFramework framework, Action runAction)
    {
        if (ThreadSafety.IsMainThread)
        {
            runAction();
            return Task.CompletedTask;
        }

        return framework.RunOnFrameworkThread(runAction);
    }
}

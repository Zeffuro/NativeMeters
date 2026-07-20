using System;
using System.Threading;
using System.Threading.Tasks;
using Dalamud.Plugin.Services;
using Dalamud.Utility;

namespace NativeMeters.Extensions;

public static class FrameworkExtensions
{
    public static Task RunSafely(this IFramework framework, Action runAction)
        => RunSafely(framework, runAction, CancellationToken.None);

    public static async Task RunSafely(this IFramework framework, Action runAction, CancellationToken cancellationToken)
    {
        if (framework.IsFrameworkUnloading)
        {
            return;
        }

        cancellationToken.ThrowIfCancellationRequested();

        if (ThreadSafety.IsMainThread)
        {
            runAction();
            return;
        }

        await framework.Run(runAction, cancellationToken);
    }

    public static async Task RunSafelyWithTimeout(
        this IFramework framework,
        Action runAction,
        CancellationToken cancellationToken,
        TimeSpan timeout)
    {
        if (framework.IsFrameworkUnloading)
        {
            return;
        }

        cancellationToken.ThrowIfCancellationRequested();

        if (ThreadSafety.IsMainThread)
        {
            runAction();
            return;
        }

        await framework.Run(runAction, cancellationToken).WaitAsync(timeout, cancellationToken);
    }
}

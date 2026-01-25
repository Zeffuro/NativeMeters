using System;
using System.Collections.Generic;

namespace NativeMeters.Extensions;

public static class DictionaryExtensions
{
    public static void DisposeValues<TKey, TValue>(this IDictionary<TKey, TValue> dict)
        where TValue : IDisposable
    {
        foreach (var value in dict.Values)
        {
            value.Dispose();
        }
        dict.Clear();
    }
}

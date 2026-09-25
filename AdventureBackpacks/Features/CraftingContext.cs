using System;
using System.Collections.Generic;
using System.Threading;

namespace AdventureBackpacks.Features;

public static class CraftingContext
{
    private static int _activeDepth = 0;
    private static readonly HashSet<string> _suppressionSources = new();
    private static readonly object _suppressionLock = new();

    public static bool IsSuppressed
    {
        get
        {
            lock (_suppressionLock)
            {
                return _suppressionSources.Count > 0;
            }
        }
    }

    public static bool IsActive => _activeDepth > 0 && !IsSuppressed;

    public readonly struct Scope : IDisposable
    {
        public void Dispose()
        {
            Exit();
        }
    }

    public static Scope Enter()
    {
        Interlocked.Increment(ref _activeDepth);
        return new Scope();
    }

    public static void Exit()
    {
        int newDepth = Interlocked.Decrement(ref _activeDepth);
        if (newDepth < 0)
        {
            Interlocked.Exchange(ref _activeDepth, 0);
        }
    }

    public static void Reset()
    {
        Interlocked.Exchange(ref _activeDepth, 0);
    }

    public static void Suppress(string modIdentifier)
    {
        if (string.IsNullOrEmpty(modIdentifier))
            return;

        lock (_suppressionLock)
        {
            _suppressionSources.Add(modIdentifier);
        }
    }

    public static void Unsuppress(string modIdentifier)
    {
        if (string.IsNullOrEmpty(modIdentifier))
            return;

        lock (_suppressionLock)
        {
            _suppressionSources.Remove(modIdentifier);
        }
    }
}

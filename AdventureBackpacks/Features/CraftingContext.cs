using System;
using System.Threading;

namespace AdventureBackpacks.Features;

public static class CraftingContext
{
    private static int _activeDepth = 0;

    public static bool IsActive => _activeDepth > 0;

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
}

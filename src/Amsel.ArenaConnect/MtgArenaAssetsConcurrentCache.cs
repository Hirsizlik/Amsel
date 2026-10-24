using System.Collections.Concurrent;
using Amsel.Data;

namespace Amsel.ArenaConnect;

public partial class MtgArenaAssetsConcurrentCache(IMtgArenaAssets parent) : IMtgArenaAssets
{
    private readonly IMtgArenaAssets parent = parent;
    private readonly Dictionary<ExpansionSymbolKey, byte[]> symbolCache = [];
    private readonly ReaderWriterLockSlim rwLock = new();

    public byte[] GetExpansionSymbolPng(ExpansionSymbolKey key)
    {
        rwLock.EnterUpgradeableReadLock();
        try
        {
            if (symbolCache.TryGetValue(key, out byte[]? result))
            {
                return result;
            }

            return LoadAndAddCacheWithWriteLock(key, out result);
        }
        finally
        {
            rwLock.ExitUpgradeableReadLock();
        }
    }

    private byte[] LoadAndAddCacheWithWriteLock(ExpansionSymbolKey key, out byte[] result)
    {
        rwLock.EnterWriteLock();
        try
        {
            result = parent.GetExpansionSymbolPng(key);
            symbolCache[key] = result;
            return result;
        }
        finally
        {
            rwLock.ExitWriteLock();
        }
    }
}

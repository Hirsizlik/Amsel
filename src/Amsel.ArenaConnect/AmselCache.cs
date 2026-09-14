using System.Collections.Immutable;
using System.IO.Compression;
using System.Text.Json;
using Amsel.Data;

namespace Amsel.ArenaConnect;

internal class AmselCache(AmselSettings settings)
{
    private interface ICache
    {
        public static abstract int CurrentVersion { get; }
        public int Version { get; }
        public DateTime Timestamp { get; }
    }

    internal record CardCache(int Version, DateTime Timestamp, ImmutableArray<CardStats> Cards) : ICache
    {
        public static int CurrentVersion { get => 0; }
    }

    internal record SetCache(int Version, DateTime Timestamp, Dictionary<string, string?> SetLocalization,
        Dictionary<string, SetMetadata> SetMetadata) : ICache
    {
        public static int CurrentVersion { get => 0; }
    }

    internal async ValueTask WriteCache(ImmutableArray<CardStats> cards,
        Dictionary<string, SetMetadata> metadata, Dictionary<string, string?> localization)
    {
        using var ccStream = new GZipStream(new FileStream(settings.CardsCacheFile, FileMode.Create),
            CompressionLevel.Optimal);
        using var scStream = new GZipStream(new FileStream(settings.SetCacheFile, FileMode.Create),
            CompressionLevel.Optimal);
        DateTime ts = DateTime.Now;
        ValueTask cardCacheTask =
            ccStream.WriteAsync(JsonSerializer.SerializeToUtf8Bytes(new CardCache(CardCache.CurrentVersion,
            ts, cards)));
        ValueTask setCacheTask =
            scStream.WriteAsync(JsonSerializer.SerializeToUtf8Bytes(new SetCache(SetCache.CurrentVersion,
            ts, localization, metadata)));
        await cardCacheTask;
        await setCacheTask;
    }

    private string GetPathForType<T>() where T : ICache
    {
        var t = typeof(T);
        if (t == typeof(CardCache))
        {
            return settings.CardsCacheFile;
        }
        else if (t == typeof(SetCache))
        {
            return settings.SetCacheFile;
        }
        throw new ArgumentException("Unknown Cache Type");
    }

    private T LoadCache<T>() where T : ICache
    {
        using var stream = new GZipStream(new FileStream(GetPathForType<T>(), FileMode.Open),
            CompressionMode.Decompress);
        T? cache = JsonSerializer.Deserialize<T>(stream);
        if (cache != null)
        {
            return cache.Version == T.CurrentVersion ?
                cache :
                throw new ArgumentException($"Invalid cache version for {typeof(T)}");
        }
        else throw new ArgumentException("Could not load data from cache");
    }

    internal async Task<(CardCache cardCache, SetCache setCache)> LoadCache()
    {
        Console.WriteLine("Loading from cache");
        CardCache cardCache = LoadCache<CardCache>();
        SetCache setCache = LoadCache<SetCache>();
        return (cardCache, setCache);
    }
}

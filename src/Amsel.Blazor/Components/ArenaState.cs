using System.Collections.Immutable;
using System.Text.Json;
using Amsel.ArenaConnect;
using Amsel.Data;
using System.IO.Compression;
using System.Collections.Frozen;

namespace Amsel.Blazor.Components;

public sealed class ArenaState(AmselSettings settings)
{
    private readonly AmselSettings settings = settings;

    public ImmutableArray<CardStats> Cards { get; private set; } = [];
    public bool FromCache { get; private set; } = false;
    public DateTime CardsLoadedTs { get; private set; }
    public FrozenDictionary<string, SetInformation> SetInformation { get; private set; }
        = FrozenDictionary.Create<string, SetInformation>([]);

    private interface ICache
    {
        public static abstract int CurrentVersion { get; }
        public int Version { get; }
        public DateTime Timestamp { get; }
    }

    private record CardCache(int Version, DateTime Timestamp, ImmutableArray<CardStats> Cards) : ICache
    {
        public static int CurrentVersion { get => 0; }
    }

    private record SetCache(int Version, DateTime Timestamp, Dictionary<string, string?> SetLocalization,
        Dictionary<string, SetMetadata> SetMetadata) : ICache
    {
        public static int CurrentVersion { get => 0; }
    }

    public async Task LoadCardInfoAsync()
    {
        if (CardsLoadedTs > DateTime.Now.AddMinutes(-5))
            return; // Cards already loaded and fresh

        await Task.Run(async () =>
        {
            try
            {
                await LoadFromArena();
            }
            catch (Exception e)
            {
                // read from cache if arena data couldn't be read
                Console.WriteLine(e.ToString());
                await LoadFromCache();
            }
        });
    }

    private static async ValueTask WriteCache(AmselSettings settings, ImmutableArray<CardStats> cards,
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

    private static FrozenDictionary<string, SetInformation> MergeIntoSetInformation(ImmutableArray<CardStats> cards,
        Dictionary<string, SetMetadata> setMetadata, Dictionary<string, string?> localization)
    {
        var setStatistics = SetStatistic.CreateStatistics(cards);
        return setStatistics
            .LeftJoin(setMetadata, st => st.Key, sm => sm.Key,
                     (st, sm) =>
                     {
                         localization.TryGetValue(st.Key, out string? name);
                         return KeyValuePair.Create(st.Key,
                            new SetInformation(st.Value, sm.Value, st.Key, name));
                     })
            .ToFrozenDictionary();
    }

    private async Task LoadFromArena()
    {
        Console.WriteLine("Loading from MTG Arena");
        IMtgArenaConnect connect = new MtgArenaConnect();
        using IMtgArenaCardDatabase cdb = new MtgArenaCardDatabase(connect.GetCardDatabasePath());
        using IMtgArenaClientLocalizationDatabase ldb = new MtgArenaClientLocalizationDatabase(connect.GetClientLocalizationDatabasePath());
        var cloc = cdb.GetEnglishLocalization();
        Dictionary<uint, CardOwned> cardsOwned = connect.GetCardsOwnedFromInventory();
        Cards = cdb.GetAllCards(cloc)
            .LeftJoin(cardsOwned, c => c.Key, o => o.Key, (c, o) => new CardStats(c.Value, o.Value?.Amount ?? 0))
            .ToImmutableArray();

        var setLocalizationDb = ldb.GetEnglishSetLocalization();
        var setMetadata = connect.GetSetMetadata();
        // use values from DB as base, and fill with Set Metadata Names if available
        Dictionary<string, string?> preparedSetLoc = new(setLocalizationDb!);
        foreach (var m in setMetadata)
        {
            if (!preparedSetLoc.ContainsKey(m.Key))
            {
                preparedSetLoc[m.Key] = m.Value.Name;
            }
        }
        FromCache = false;
        CardsLoadedTs = DateTime.Now;

        SetInformation = MergeIntoSetInformation(Cards, setMetadata, preparedSetLoc);
        await WriteCache(settings, Cards, setMetadata, preparedSetLoc);
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

    private async Task LoadFromCache()
    {
        Console.WriteLine("Loading from cache");
        CardCache cardCache = LoadCache<CardCache>();
        SetCache setCache = LoadCache<SetCache>();

        Cards = cardCache.Cards;
        SetInformation = MergeIntoSetInformation(Cards, setCache.SetMetadata, setCache.SetLocalization);
        CardsLoadedTs = DateTime.Now;
        FromCache = true;
    }
}

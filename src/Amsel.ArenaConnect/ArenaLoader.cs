using System.Collections.Immutable;
using Amsel.Data;
using System.Collections.Frozen;

namespace Amsel.ArenaConnect;

public sealed class ArenaLoader(AmselSettings settings)
{
    private readonly AmselCache cache = new(settings);

    public ImmutableArray<CardStats> Cards { get; private set; } = [];
    public bool FromCache { get; private set; } = false;
    public DateTime CardsLoadedTs { get; private set; }
    public FrozenDictionary<string, SetInformation> SetInformation { get; private set; }
        = FrozenDictionary.Create<string, SetInformation>([]);
    public FrozenDictionary<uint, int> OwnedPerTitle = FrozenDictionary.Create<uint, int>([]);
    public ImmutableArray<FormatInformation> FormatInfo = [];

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

    private async Task LoadFromCache()
    {
        var (cardCache, setCache, formatCache) = await cache.LoadCache();
        Cards = cardCache.Cards;
        SetInformation = MergeIntoSetInformation(Cards, setCache.SetMetadata, setCache.SetLocalization);
        OwnedPerTitle = CountOwnedPerTitleId(Cards);
        FormatInfo = formatCache.FormatInfo;
        CardsLoadedTs = DateTime.Now;
        FromCache = true;
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

    private static FrozenDictionary<uint, int> CountOwnedPerTitleId(ImmutableArray<CardStats> cards)
    {
        return cards
            .GroupBy(c => c.Info.TitleId)
            .AggregateBy(g => g.Key, 0, (acc, rhs) => acc + rhs.Sum(c => c.Owned))
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

        var formatLocalizationDb = ldb.GetEnglishFormatLocalization();
        FormatInfo = connect.GetFormatData()
            .Select(f => new FormatInformation(f, formatLocalizationDb[f.NameKey]))
            .ToImmutableArray();
        SetInformation = MergeIntoSetInformation(Cards, setMetadata, preparedSetLoc);
        OwnedPerTitle = CountOwnedPerTitleId(Cards);
        await cache.WriteCache(Cards, setMetadata, preparedSetLoc, FormatInfo);
    }
}

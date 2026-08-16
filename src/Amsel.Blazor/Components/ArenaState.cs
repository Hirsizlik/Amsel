using System.Collections.Immutable;
using System.Text.Json;
using Amsel.ArenaConnect;
using Amsel.Data;
using System.IO.Compression;

namespace Amsel.Blazor.Components;

public sealed class ArenaState(AmselSettings settings)
{
    private readonly AmselSettings settings = settings;

    public ImmutableArray<CardStats> Cards { get; private set; } = [];
    public bool FromCache { get; private set; } = false;
    public DateTime CardsLoadedTs { get; private set; }

    private record CardCache(int Version, DateTime Timestamp, ImmutableArray<CardStats> Cards)
    {
        public const int CurrentVersion = 0;
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

    private async Task LoadFromArena()
    {
        Console.WriteLine("Loading from MTG Arena");
        IMtgArenaConnect connect = new MtgArenaConnect();
        using IMtgArenaDatabase db = new MtgArenaDatabase(connect.GetDatabasePath());
        var loc = db.GetEnglishLocalization();
        Dictionary<uint, CardOwned> cardsOwned = connect.GetCardsOwnedFromInventory();
        Cards = db.GetAllCards(loc)
            .LeftJoin(cardsOwned, c => c.Key, o => o.Key, (c, o) => new CardStats(c.Value, o.Value?.Amount ?? 0))
            .ToImmutableArray();
        FromCache = false;
        CardsLoadedTs = DateTime.Now;
        using var stream = new GZipStream(new FileStream(settings.CardsCacheFile, FileMode.Create),
            CompressionLevel.Optimal);
        await stream.WriteAsync(JsonSerializer.SerializeToUtf8Bytes(
            new CardCache(CardCache.CurrentVersion, DateTime.Now, Cards)));
    }

    private async Task LoadFromCache()
    {
        Console.WriteLine("Loading from cache");
        using var stream = new GZipStream(new FileStream(settings.CardsCacheFile, FileMode.Open),
            CompressionMode.Decompress);
        CardCache? cache = JsonSerializer.Deserialize<CardCache>(stream);
        if (cache != null)
        {
            if (cache.Version == CardCache.CurrentVersion)
            {
                Cards = cache.Cards;
                CardsLoadedTs = DateTime.Now;
                FromCache = true;
            }
            else
            {
                throw new ArgumentException("Invalid cache version");
            }
        }
        else
        {
            throw new ArgumentException("Could not load data from cache");
        }
    }
}

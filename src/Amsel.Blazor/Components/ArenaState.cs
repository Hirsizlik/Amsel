using System.Collections.Immutable;
using Amsel.ArenaConnect;

namespace Amsel.Blazor.Components;

public record CardStats(CardInfo Info, int Owned);

public sealed class ArenaState
{
    public ImmutableArray<CardStats> Cards { get; private set; } = [];

    public async Task LoadCardInfoAsync()
    {
        await Task.Run(() =>
        {
            IMtgArenaConnect connect = new MtgArenaConnect();
            using IMtgArenaDatabase db = new MtgArenaDatabase(connect.GetDatabasePath());
            var loc = db.GetEnglishLocalization();
            Dictionary<uint, CardOwned> cardsOwned = connect.GetCardsOwnedFromInventory();
            Cards = db.GetAllCards(loc)
                .LeftJoin(cardsOwned, c => c.Key, o => o.Key, (c, o) => new CardStats(c.Value, o.Value?.Amount ?? 0))
                .ToImmutableArray();
        });
    }
}

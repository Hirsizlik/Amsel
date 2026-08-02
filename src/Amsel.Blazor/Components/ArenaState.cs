using Amsel.ArenaConnect;

namespace Amsel.Blazor.Components;

public sealed class ArenaState
{
    public Dictionary<uint, CardInfo> Cards { get; private set; } = [];
    public Dictionary<uint, CardOwned> CardsOwned { get; private set; } = [];

    public void LoadCardInfo()
    {
        IMtgArenaConnect connect = new MtgArenaConnect();
        using IMtgArenaDatabase db = new MtgArenaDatabase(connect.GetDatabasePath());
        var loc = db.GetEnglishLocalization();
        Cards = db.GetAllCards(loc);
        CardsOwned = connect.GetCardsOwnedFromInventory();
    }
}

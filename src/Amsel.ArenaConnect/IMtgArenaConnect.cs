namespace Amsel.ArenaConnect;

public interface IMtgArenaConnect
{
    Dictionary<uint, CardOwned> GetCardsOwnedFromInventory();
    string GetDatabasePath();
}

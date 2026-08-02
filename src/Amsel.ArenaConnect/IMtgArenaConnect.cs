namespace Amsel.ArenaConnect;

public interface IMtgArenaConnect
{
    List<CardOwned> GetCardsOwnedFromInventory();
    string GetDatabasePath();
}

using Amsel.Data;

namespace Amsel.ArenaConnect;

public interface IMtgArenaConnect
{
    Dictionary<uint, CardOwned> GetCardsOwnedFromInventory();
    string GetCardDatabasePath();
    Dictionary<string, SetMetadata> GetSetMetadata();
    string GetClientLocalizationDatabasePath();
}

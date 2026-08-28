using Amsel.Data;

namespace Amsel.ArenaConnect;

public interface IMtgArenaCardDatabase : IDisposable
{
    Dictionary<uint, string> GetEnglishLocalization();
    Dictionary<uint, CardInfo> GetAllCards(Dictionary<uint, string> localizations, bool onlyPrimary = true);
}

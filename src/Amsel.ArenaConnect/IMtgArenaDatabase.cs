namespace Amsel.ArenaConnect;

public interface IMtgArenaDatabase : IDisposable
{
    Dictionary<uint, string> GetEnglishLocalization();
    Dictionary<uint, CardInfo> GetAllCards(Dictionary<uint, string> Localizations);
}

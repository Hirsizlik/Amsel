namespace Amsel.ArenaConnect;

public interface IMtgArenaClientLocalizationDatabase : IDisposable
{
    Dictionary<string, string> GetEnglishSetLocalization();
}

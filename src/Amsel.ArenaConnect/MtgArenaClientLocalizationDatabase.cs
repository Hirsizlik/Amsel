using Microsoft.Data.Sqlite;

namespace Amsel.ArenaConnect;

public sealed class MtgArenaClientLocalizationDatabase : IMtgArenaClientLocalizationDatabase
{
    private readonly SqliteConnection connection;

    public MtgArenaClientLocalizationDatabase(string path)
    {
        connection = new($"Data Source={path};Mode=ReadOnly");
        connection.Open();
    }

    public Dictionary<string, string> GetEnglishSetLocalization()
    {
        using var command = connection.CreateCommand();
        command.CommandText = """
        SELECT SUBSTR(l.Key, 14), l.enUS FROM Loc l WHERE l.Key LIKE 'General/Sets/%'
        """;
        using var reader = command.ExecuteReader();
        Dictionary<string, string> result = [];

        while (reader.Read())
        {
            var locKey = reader.GetString(0);
            var enUs = reader.GetString(1);
            result.Add(locKey, enUs);
        }
        return result;
    }

    public Dictionary<string, string> GetEnglishFormatLocalization()
    {
        using var command = connection.CreateCommand();
        command.CommandText = """
        SELECT SUBSTR(l.Key, 20), l.enUS FROM Loc l WHERE l.Key LIKE 'MainNav/DeckFormat/%'
        """;
        using var reader = command.ExecuteReader();
        Dictionary<string, string> result = [];

        while (reader.Read())
        {
            var locKey = reader.GetString(0);
            var enUs = reader.GetString(1);
            result.Add(locKey, enUs);
        }
        return result;
    }

    public void Dispose()
    {
        connection.Dispose();
    }
}

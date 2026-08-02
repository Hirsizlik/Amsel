using Microsoft.Data.Sqlite;

namespace Amsel.ArenaConnect;

public sealed class MtgArenaDatabase : IMtgArenaDatabase
{
    private readonly SqliteConnection connection;

    public MtgArenaDatabase(string path)
    {
        connection = new($"Data Source={path};Mode=ReadOnly");
        connection.Open();
    }

    public Dictionary<uint, string> GetEnglishLocalization()
    {
        using var command = connection.CreateCommand();
        command.CommandText = """
        SELECT l.LocId, l.Formatted, l.Loc FROM Localizations_enUS l ORDER BY l.LocId, l.Formatted
        """;
        using var reader = command.ExecuteReader();
        Dictionary<uint, string> result = [];
        uint currentLocId = 0;
        while (reader.Read())
        {
            var locId = reader.GetInt32(0);
            if (locId == currentLocId)
            {
                continue; // ignore, unformatted already found
            }
            currentLocId = (uint)locId;
            var loc = reader.GetString(2);

            result.Add(currentLocId, loc);
        }
        return result;
    }

    public Dictionary<uint, CardInfo> GetAllCards(Dictionary<uint, string> Localizations)
    {
        using var command = connection.CreateCommand();
        command.CommandText = """
        SELECT c.GrpId, c.TitleId, c.ExpansionCode, c.DigitalReleaseSet, c.CollectorNumber, c.CollectorMax, c.Rarity
        FROM Cards c
        """;
        using var reader = command.ExecuteReader();
        Dictionary<uint, CardInfo> result = [];
        while (reader.Read())
        {
            uint id = (uint)reader.GetInt32(0);
            uint titleId = (uint)reader.GetInt32(1);
            if (titleId == 0)
                continue;

            result.Add(id, new CardInfo
            (
                id,
                Localizations[titleId],
                reader.GetString(2),
                reader.GetString(3),
                (uint)reader.GetInt32(4),
                reader.GetValue(5) as uint?,
                (Rarity)reader.GetInt32(6)
            ));
        }
        return result;
    }

    public void Dispose()
    {
        connection.Dispose();
    }
}

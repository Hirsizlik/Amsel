using Microsoft.Data.Sqlite;
using Amsel.Data;
using System.Collections.Immutable;

namespace Amsel.ArenaConnect;

public sealed class MtgArenaCardDatabase : IMtgArenaCardDatabase
{
    private readonly SqliteConnection connection;

    public MtgArenaCardDatabase(string path)
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

    public Dictionary<uint, CardInfo> GetAllCards(Dictionary<uint, string> localizations, bool onlyPrimary)
    {
        using var command = connection.CreateCommand();
        command.CommandText = """
        SELECT c.GrpId, c.TitleId, c.ExpansionCode, c.DigitalReleaseSet,
            c.CollectorNumber, c.CollectorMax, c.Rarity, c.IsPrimaryCard, c.SuperTypes
        FROM Cards c
        """;
        if (onlyPrimary)
        {
            command.CommandText += " WHERE c.IsPrimaryCard = 1";
        }
        using var reader = command.ExecuteReader();
        Dictionary<uint, CardInfo> result = [];
        while (reader.Read())
        {
            uint titleId = (uint)reader.GetInt32(1);
            if (titleId == 0)
                continue;
            uint id = (uint)reader.GetInt32(0);
            int collectorMax = reader.GetInt32(5);
            result.Add(id, new CardInfo
            (
                id,
                localizations[titleId],
                reader.GetString(2),
                reader.GetString(3),
                (uint)reader.GetInt32(4),
                collectorMax != 0 ? (uint?)collectorMax : null,
                (Rarity)reader.GetInt32(6),
                reader.GetBoolean(7),
                titleId,
                [.. reader.GetString(8).Split(",")
                    .Where(s => !string.IsNullOrWhiteSpace(s))
                    .Select(i => uint.Parse(i))]
            ));
        }
        return result;
    }

    public void Dispose()
    {
        connection.Dispose();
    }
}

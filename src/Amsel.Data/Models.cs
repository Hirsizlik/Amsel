namespace Amsel.Data;

public enum Rarity
{
    Unknown,
    Land,
    Common,
    Uncommon,
    Rare,
    MythicRare
}

public static class RarityExtension
{
    extension(Rarity r)
    {
        public char ToChar()
        {
            return r switch
            {
                Rarity.Land => 'L',
                Rarity.Common => 'C',
                Rarity.Uncommon => 'U',
                Rarity.Rare => 'R',
                Rarity.MythicRare => 'M',
                _ => '?'
            };
        }
    }

    public static Rarity FromChar(char c)
    {
        return c switch
        {
            'L' => Rarity.Land,
            'C' => Rarity.Common,
            'U' => Rarity.Uncommon,
            'R' => Rarity.Rare,
            'M' => Rarity.MythicRare,
            _ => Rarity.Uncommon
        };
    }
}

public record CardOwned(uint CardId, int Amount);

public record CardInfo(uint CardId, string Name, string ExpansionCode, string DigitalReleaseSet,
    uint CollectorNumber, uint? CollectorMax, Rarity Rarity, bool IsPrimary);

public record CardStats(CardInfo Info, int Owned);

public enum Availability
{
    EternalOnly, // Paper sets out of Rotation
    StandardNotAlchemy, // Sets in Standard, but not in Alchemy (3 Years)
    Available, // Standard + Alchemy (2 Years) Sets
    AlchemyNotStandard, // Current Alchemy sets
    HistoricOnly, // Digital only sets not in Alchemy (PIO, SIR, rotated Alchemy sets)
    RotatingOutSoonStandard, // Rotating from StandardNotAlchemy to EternalOnly
    RotatingOutSoonAlchemy // Rotating from Alchemy to Standard or Historic
}

// Name and ParentCode are only set for related sub sets (IsMajorCardSet = false)
public record SetMetadata(int CollationId, string Code, DateTime ReleaseDate, bool IsMajorCardSet,
                          Availability Availability, string? Name, string? ParentCode);

public record SetInformation(SetStatistic Statistic, SetMetadata? Metadata,
    string Code, string? Name);

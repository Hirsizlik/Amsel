using System.Collections.Frozen;
using System.Collections.Immutable;

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
        return char.ToUpperInvariant(c) switch
        {
            'L' => Rarity.Land,
            'C' => Rarity.Common,
            'U' => Rarity.Uncommon,
            'R' => Rarity.Rare,
            'M' => Rarity.MythicRare,
            _ => throw new ArgumentException($"Unknown rarity char '{c}'")
        };
    }
}

public record CardOwned(uint CardId, int Amount);

public record CardInfo(uint CardId, string Name, string ExpansionCode, string DigitalReleaseSet,
    uint CollectorNumber, uint? CollectorMax, Rarity Rarity, bool IsPrimary, uint TitleId,
    ImmutableArray<uint> Supertypes);

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

public readonly record struct Quota(uint Max);
public record FormatData(string NameKey, FrozenSet<string> LegalSets, FrozenSet<uint> BannedTitleIds,
    FrozenDictionary<uint, Quota> RestrictedTitleIds, FrozenSet<uint> BannedAsCommanderTitles,
    FrozenSet<uint> LegalTitleIds)
{
    public FormatDataThawed Thaw()
    {
        return new FormatDataThawed(NameKey, [.. LegalSets], [.. BannedTitleIds],
            RestrictedTitleIds.ToDictionary(), [.. BannedAsCommanderTitles],
            [.. LegalTitleIds]);
    }
}

public record FormatDataThawed(string NameKey, HashSet<string> LegalSets, HashSet<uint> BannedTitleIds,
    Dictionary<uint, Quota> RestrictedTitleIds, HashSet<uint> BannedAsCommanderTitles,
    HashSet<uint> LegalTitleIds)
{
    public FormatData Freeze()
    {
        return new FormatData(NameKey, LegalSets.ToFrozenSet(), BannedTitleIds.ToFrozenSet(),
            RestrictedTitleIds.ToFrozenDictionary(), BannedAsCommanderTitles.ToFrozenSet(),
            LegalTitleIds.ToFrozenSet());
    }
}

public record FormatInformation(FormatData Data, string Name)
{
    public FormatInformationThawed Thaw()
    {
        return new FormatInformationThawed(Data.Thaw(), Name);
    }
}

public record FormatInformationThawed(FormatDataThawed Data, string Name)
{
    public FormatInformation Freeze()
    {
        return new FormatInformation(Data.Freeze(), Name);
    }
}

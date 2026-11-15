using System.Collections.Generic;
using System.Collections.Immutable;
using System.IO;
using System.Linq;
using Amsel.ArenaConnect;
using Amsel.Data;
using Avalonia.Media;
using Avalonia.Media.Imaging;

namespace Amsel.Avalonia.Models;

public class DecoratedSetInformation(
    SetInformation set,
    IMtgArenaAssets assets,
    ImmutableArray<FormatInformation> formatInformation)
{
    public static readonly DecoratedSetInformation Empty = new(
        new SetInformation(new SetStatistic(), null, "", null), null!, []);

    public SetInformation Set { get; init; } = set;

    public IImage? CommonSymbol { get => GetSymbol(Rarity.Common); }
    public IImage? UncommonSymbol { get => GetSymbol(Rarity.Uncommon); }
    public IImage? RareSymbol { get => GetSymbol(Rarity.Rare); }
    public IImage? MythicSymbol { get => GetSymbol(Rarity.MythicRare); }

    public bool HasMetadata { get => Set.Metadata != null; }
    public bool HasNoMetadataNoName { get => Set.Metadata == null && Set.Name == null; }
    public bool HasOnlyName { get => Set.Metadata == null && Set.Name != null; }
    public bool IsMajorSet { get => Set.Metadata != null && Set.Metadata.ParentCode == null; }
    public bool IsSubSet { get => Set.Metadata?.ParentCode != null; }
    public bool Completed4x { get => Set.Statistic.TotalOwned == Set.Statistic.CardCount * 4; }
    public bool Completed1x { get => Set.Statistic.TotalOwnedUnique == Set.Statistic.CardCount; }
    public bool Incomplete { get => Set.Statistic.TotalOwnedUnique != Set.Statistic.CardCount; }
    public bool HasFormatLegalities { get => FormatLegalities.Length > 0; }

    public string? ReleaseDateString { get => Set.Metadata?.ReleaseDate.ToShortDateString(); }

    public int CardCount4x { get => Set.Statistic.CardCount * 4; }

    public int CommonCardCount { get => Set.Statistic.GetCardCount(Rarity.Common); }
    public int CommonCardCount4x { get => CommonCardCount * 4; }
    public int CommonOwned { get => Set.Statistic.GetAmountTotal(Rarity.Common); }
    public int CommonOwnedUnique { get => Set.Statistic.GetAmountUnique(Rarity.Common); }
    public bool HasCommons { get => CommonCardCount > 0; }

    public int UncommonCardCount { get => Set.Statistic.GetCardCount(Rarity.Uncommon); }
    public int UncommonCardCount4x { get => UncommonCardCount * 4; }
    public int UncommonOwned { get => Set.Statistic.GetAmountTotal(Rarity.Uncommon); }
    public int UncommonOwnedUnique { get => Set.Statistic.GetAmountUnique(Rarity.Uncommon); }
    public bool HasUncommons { get => UncommonCardCount > 0; }

    public int RareCardCount { get => Set.Statistic.GetCardCount(Rarity.Rare); }
    public int RareCardCount4x { get => RareCardCount * 4; }
    public int RareOwned { get => Set.Statistic.GetAmountTotal(Rarity.Rare); }
    public int RareOwnedUnique { get => Set.Statistic.GetAmountUnique(Rarity.Rare); }
    public bool HasRares { get => RareCardCount > 0; }

    public int MythicRareCardCount { get => Set.Statistic.GetCardCount(Rarity.MythicRare); }
    public int MythicRareCardCount4x { get => MythicRareCardCount * 4; }
    public int MythicRareOwned { get => Set.Statistic.GetAmountTotal(Rarity.MythicRare); }
    public int MythicRareOwnedUnique { get => Set.Statistic.GetAmountUnique(Rarity.MythicRare); }
    public bool HasMythics { get => MythicRareCardCount > 0; }

    public FormatLegality[] FormatLegalities { get; init; } = InitFormatLegalities(set, formatInformation);

    private static FormatLegality[] InitFormatLegalities(
        SetInformation set,
        ImmutableArray<FormatInformation> formatInformation)
    {
        return formatInformation
            .Where(f => f.Data.LegalSets.Contains(set.Code))
            .Select(f => new FormatLegality(f.Name, false, null))
            .ToArray();
    }

    private IImage? GetSymbol(Rarity r)
    {
        if (this == Empty)
        {
            return null;
        }
        var key = new ExpansionSymbolKey(Set.Code, r);
        var png = assets.GetExpansionSymbolPng(key);
        return new Bitmap(new MemoryStream(png));
    }
}


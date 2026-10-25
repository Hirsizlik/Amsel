using System;
using System.Collections.Frozen;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.IO;
using System.Linq;
using Amsel.ArenaConnect;
using Amsel.Data;
using Avalonia.Media;
using Avalonia.Media.Imaging;

namespace Amsel.Avalonia.Models;

public class DecoratedCardStats(
    CardStats cardStats,
    SetInformation set,
    SetInformation? digitalSet,
    IMtgArenaAssets assets,
    FrozenDictionary<uint, int> ownedPerTitle,
    ImmutableArray<FormatInformation> formatInformation)
{
    public CardStats CardStats { get; init; } = cardStats;
    public IImage SetSymbol
    {
        get
        {
            var key = new ExpansionSymbolKey(CardStats.Info.ExpansionCode, CardStats.Info.Rarity);
            var png = assets.GetExpansionSymbolPng(key);
            return new Bitmap(new MemoryStream(png));
        }
    }

    public IImage DigitalSetSymbol
    {
        get
        {
            var key = new ExpansionSymbolKey(CardStats.Info.DigitalReleaseSet, CardStats.Info.Rarity);
            var png = assets.GetExpansionSymbolPng(key);
            return new Bitmap(new MemoryStream(png));
        }
    }

    public string? SetName { get; init; } = set.Name;
    public string? DigitalSetName { get; init; } = digitalSet?.Name;
    public char RarityCode { get => CardStats.Info.Rarity.ToChar(); }
    public int OwnedOverall { get => ownedPerTitle[CardStats.Info.TitleId]; }
    public FormatLegality[] FormatLegalities { get; init; } = InitFormatLegalities(cardStats, formatInformation);

    private static FormatLegality[] InitFormatLegalities(
        CardStats cardStats,
        ImmutableArray<FormatInformation> formatInformation)
    {
        List<FormatLegality> result = [];
        foreach (var f in formatInformation)
        {
            if (f.Data.RestrictedTitleIds.TryGetValue(cardStats.Info.TitleId, out var quota))
            {
                result.Add(new(f.Name, false, $"max. {quota.Max}"));
            }
            else if (f.Data.LegalTitleIds.Contains(cardStats.Info.TitleId))
            {
                result.Add(new(f.Name, false, null));
            }
            else if (f.Data.BannedAsCommanderTitles.Contains(cardStats.Info.TitleId))
            {
                result.Add(new(f.Name, true, "as Commander"));
            }
            else if (f.Data.BannedTitleIds.Contains(cardStats.Info.TitleId))
            {
                result.Add(new(f.Name, true, null));
            }
        }
        return result.ToArray();
    }
}

using System.IO;
using Amsel.ArenaConnect;
using Amsel.Data;
using Avalonia.Media;
using Avalonia.Media.Imaging;

namespace Amsel.Avalonia.Models;

public class DecoratedSetInformation(SetInformation set, IMtgArenaAssets assets)
{
    public static readonly DecoratedSetInformation Empty = new(
        new SetInformation(new SetStatistic(), null, "", null), null!);

    public SetInformation Set { get; init; } = set;

    public IImage? CommonSymbol
    {
        get
        {
            if (this == Empty)
            {
                return null;
            }
            var key = new ExpansionSymbolKey(Set.Code, Rarity.Common);
            var png = assets.GetExpansionSymbolPng(key);
            return new Bitmap(new MemoryStream(png));
        }
    }

    public bool HasMetadata { get => Set.Metadata != null; }
    public bool HasNoMetadataNoName { get => Set.Metadata == null && Set.Name == null; }
    public bool HasOnlyName { get => Set.Metadata == null && Set.Name != null; }
    public bool IsMajorSet { get => Set.Metadata != null && Set.Metadata.ParentCode == null; }
    public bool IsSubSet { get => Set.Metadata?.ParentCode != null; }
    public bool Completed4x { get => Set.Statistic.TotalOwned == Set.Statistic.CardCount * 4; }
    public bool Completed1x { get => Set.Statistic.TotalOwnedUnique == Set.Statistic.CardCount; }
    public bool Incomplete { get => Set.Statistic.TotalOwnedUnique != Set.Statistic.CardCount; }

    public string? ReleaseDateString { get => Set.Metadata?.ReleaseDate.ToShortDateString(); }

    public int CardCount4x { get => Set.Statistic.CardCount * 4; }
}


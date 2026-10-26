using System.IO;
using Amsel.ArenaConnect;
using Amsel.Data;
using Avalonia.Media;
using Avalonia.Media.Imaging;

namespace Amsel.Avalonia.Models;

public class DecoratedSetInformation(SetInformation set, IMtgArenaAssets assets)
{
    public static DecoratedSetInformation Empty = new(
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
}



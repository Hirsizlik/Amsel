using Amsel.Data;

namespace Amsel.ArenaConnect;

public readonly record struct Rect(int X, int Y, int Width, int Height);
public readonly record struct TextureData(byte[] Bgra32, int Width, int Height, Rect? Crop);

public interface IMtgArenaAssets
{
    TextureData GetExpansionSymbol(string expansionCode, Rarity rarity);

}

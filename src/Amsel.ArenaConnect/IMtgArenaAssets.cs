using Amsel.Data;

namespace Amsel.ArenaConnect;

public readonly record struct TextureData(byte[] Bgra32, int Width, int Height, 
    bool Crop, int CropX, int CropY, int CropWidth, int CropHeight);

public interface IMtgArenaAssets
{
    TextureData GetExpansionSymbol(string expansionCode, Rarity rarity);

}

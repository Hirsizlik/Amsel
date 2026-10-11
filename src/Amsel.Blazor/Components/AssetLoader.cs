using Amsel.ArenaConnect;
using Amsel.Data;

namespace Amsel.Blazor.Components;

public class AssetLoader(ArenaLoader loader)
{
    private MtgArenaAssetsConcurrentCache? arenaAssets;

    public async Task<byte[]> GetExpansionSymbol(string code, Rarity rarity)
    {
        if (arenaAssets == null)
        {
            if (loader.DataDir == null)
            {
                await loader.LoadCardInfoAsync();
            }

            arenaAssets ??= new MtgArenaAssetsConcurrentCache(
                MtgArenaAssets.Init(loader.DataDir ?? throw new Exception("No data dir")));
        }

        return arenaAssets.GetExpansionSymbolPng(new ExpansionSymbolKey(code, rarity));
    }
}

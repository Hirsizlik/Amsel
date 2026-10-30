using Amsel.ArenaConnect;

public class MtgArenaAssetsFactory
{
    private readonly ArenaLoader loader;

    private IMtgArenaAssets? arenaAssets;

    public MtgArenaAssetsFactory(ArenaLoader loader)
    {
        this.loader = loader;
    }

    public IMtgArenaAssets Create()
    {
        arenaAssets ??= new MtgArenaAssetsConcurrentCache(
            MtgArenaAssets.Init(loader.DataDir ?? throw new ArgumentNullException("No DataDir")));
        return arenaAssets;
    }
}

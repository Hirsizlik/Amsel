namespace Amsel.ArenaConnect;

public sealed record AmselSettings(string CacheDir)
{
    public string CardsCacheFile { get => CacheDir + "/CardCache.json.gz"; }
    public string SetCacheFile { get => CacheDir + "/SetCache.json.gz"; }
    public string FormatCacheFile { get => CacheDir + "/FormatCache.json.gz"; }
}

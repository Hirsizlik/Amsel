using System.Collections.Concurrent;
using System.Runtime.InteropServices;
using Amsel.ArenaConnect;
using Amsel.Data;
using SkiaSharp;

namespace Amsel.Blazor.Components;

public class AssetLoader(ArenaLoader loader)
{
    private readonly record struct ExpansionSymbolCacheKey(string Code, Rarity Rarity);

    private MtgArenaAssets? arenaAssets;
    private readonly ConcurrentDictionary<ExpansionSymbolCacheKey, byte[]> symbolCache = [];

    private static SKBitmap LoadTexture(TextureData tex)
    {
        var source = new SKBitmap();
        var gcHandle = GCHandle.Alloc(tex.Bgra32, GCHandleType.Pinned);
        var info = new SKImageInfo(tex.Width, tex.Height, SKImageInfo.PlatformColorType, SKAlphaType.Unpremul);
        source.InstallPixels(info, gcHandle.AddrOfPinnedObject(), info.RowBytes, delegate { gcHandle.Free(); });
        return source;
    }

    public async Task<byte[]> GetExpansionSymbol(string code, Rarity rarity)
    {
        if (arenaAssets == null)
        {
            if (loader.DataDir == null)
            {
                await loader.LoadCardInfoAsync();
            }

            arenaAssets ??= MtgArenaAssets.Init(loader.DataDir ?? throw new Exception("No data dir"));
        }
        code = SetRaritySymbolFixup.FixUpCode(code);
        rarity = SetRaritySymbolFixup.FixUpRarity(code, rarity);
        var key = new ExpansionSymbolCacheKey(code, rarity);
        if (!symbolCache.TryGetValue(key, out byte[]? result))
        {
            TextureData tex = arenaAssets.GetExpansionSymbol(code, rarity);

            SKBitmap source = LoadTexture(tex);
            Rect rect = GetDimensions(tex);
            SKBitmap destination = new(rect.Width, rect.Height);
            FlipCrop(source, destination, rect);
            SKImage image = SKImage.FromBitmap(destination);
            var data = image.Encode(SKEncodedImageFormat.Png, 100);
            result = data.ToArray();
            symbolCache[key] = result;
        }

        return result;
    }

    private static Rect GetDimensions(TextureData tex)
    {
        if (tex.Crop is { } c)
        {
            return c;
        }
        else
        {
            return new Rect(0, 0, tex.Width, tex.Height);
        }
    }

    private static void FlipCrop(SKBitmap source, SKBitmap destination, Rect rect)
    {
        using SKCanvas canvas = new(destination);
        canvas.RotateDegrees(180, rect.Width / 2, rect.Height / 2);
        canvas.Translate(rect.Width, 0);
        canvas.Scale(-1, 1);
        canvas.DrawBitmap(source, rect.X, -rect.Y, SKSamplingOptions.Default);
    }
}

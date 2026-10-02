using System.Collections.Frozen;
using Amsel.ArenaConnect;
using Amsel.Data;
using SixLabors.ImageSharp;
using SixLabors.ImageSharp.PixelFormats;
using SixLabors.ImageSharp.Processing;

namespace Amsel.Blazor.Components;

public class AssetLoader(ArenaLoader loader)
{
    private readonly record struct ExpansionSymbolCacheKey(string Code, Rarity Rarity);

    private MtgArenaAssets? arenaAssets;
    private readonly Dictionary<ExpansionSymbolCacheKey, byte[]> symbolCache = [];
    private FrozenSet<string> ReverseSets = FrozenSet.Create(["FACE", "MAR", "LIST", "BRAWL"]);

    private string FixUpCode(string code)
    {
        if (code.Split('-') is [var first, var second, ..])
        {
            code = ReverseSets.Contains(first) ? second : first;
        }
        return code switch
        {
            "SLD" or "APRM" or "EXTRALIFE2025" or "EUROLANDS" or "APACLANDS" or "PLANECATION" => "PRM", // Secred lair and other Promos
            "G18" => "M19", // M19 gift pack
            "MAR" => "MSC", // MAR logo is not included (because red and usually not visible?)
            "AEFA" => "MH2", // MH2 Fetch Lands
            "YECL" => "LRW", // Lorwyn Eclipsed (use Lorwyn logo)
            "CONF" => "CON", // Conflux
            "SUMMER" => "INR", // "SUMMER-51.0", Land Promos from Innistrad Remastered
            // Alpha, Arena Beginner, Elspeth vs Ashiok, Cube cards
            "LEA" or "ANA" or "ANB" or "TGA19" or "CUBE" => "ARENA",
            // Arena Historic Anthologies
            string c when c.StartsWith("AHA") => "ARENA",
            // Arena, Explorer, Pioneer Anthologies
            string c when (c.StartsWith("AA") || c.StartsWith("EA") || c.StartsWith("PA"))
                && char.IsDigit(c[2]) => "ARENA",
            _ => code
        };
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
        code = FixUpCode(code);
        var key = new ExpansionSymbolCacheKey(code, rarity);
        if (!symbolCache.TryGetValue(key, out byte[]? result))
        {
            TextureData tex = arenaAssets.GetExpansionSymbol(code, rarity);
            var image = Image.LoadPixelData<Bgra32>(tex.Bgra32, tex.Width, tex.Height);
            if (tex.Crop != null)
            {
                var c = tex.Crop.Value;
                image.Mutate(i => i.Crop(new Rectangle(c.X, c.Y, c.Width, c.Height)));
            }
            image.Mutate(i => i.Flip(FlipMode.Vertical));
            MemoryStream ms = new();
            await image.SaveAsPngAsync(ms);
            result = ms.ToArray();
            symbolCache[key] = result;
        }

        return result;
    }
}

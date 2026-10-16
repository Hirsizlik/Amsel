using System.Collections.Concurrent;
using System.Collections.Frozen;
using System.Collections.Immutable;
using System.Text.Json;
using System.Text.RegularExpressions;
using Amsel.Data;
using AssetsTools.NET;
using AssetsTools.NET.Extra;
using AssetsTools.NET.Texture;
using SkiaSharp;

namespace Amsel.ArenaConnect;

public partial class MtgArenaAssets : IMtgArenaAssets
{
    private record Manifest(ImmutableArray<ManifestEntry> Assets);
    private record ManifestEntry(string Name, int Length, int Priority, string sha256, string crc,
        ImmutableArray<string> IndexedAssets);
    private record ExpansionCodeRarityKey(string ExpansionCode, Rarity Rarity);
    private record ExpansionCodeRarityValue(string BundleName, string Path);

    // some use "ExpansionSymbols" with s, most have none
    // Y26-Common has a typo, including an extra whitespace
    [GeneratedRegex("Images/ExpansionSymbols?_(\\w{3,})_ ?(Common|Uncommon|Rare|Mythic)\\.png$")]
    private static partial Regex ExpansionSymbolPathPattern { get; }

    private FrozenDictionary<ExpansionCodeRarityKey, ExpansionCodeRarityValue> expansionCodeRarityToBundleName;
    private readonly FrozenDictionary<string, object> bundleLocks;
    private readonly AssetsManager manager = new();
    private readonly string downloadsDir;
    private readonly ConcurrentDictionary<string, BundleFileInstance> openBundleFiles = [];
    private readonly ConcurrentDictionary<string, FrozenDictionary<long, string>> bundleNameToPathIdMappings = [];

    private MtgArenaAssets(
        FrozenDictionary<ExpansionCodeRarityKey, ExpansionCodeRarityValue> expansionCodeRarityToBundleName,
        FrozenDictionary<string, object> bundleLocks,
        string downloadsDir)
    {
        this.expansionCodeRarityToBundleName = expansionCodeRarityToBundleName;
        this.bundleLocks = bundleLocks;
        this.downloadsDir = downloadsDir;
    }

    private static string GetManifestPath(string downloadsDir)
    {
        foreach (string file in Directory.GetFiles(downloadsDir, "Manifest_*.mtga", SearchOption.TopDirectoryOnly))
        {
            // the main Manifest has only one underscore
            if (file.Split('/')[^1].Count('_') == 1)
            {
                return file;
            }
        }
        throw new Exception("Manifest not found");
    }

    private static Rarity ParseRarity(string path)
    {
        return path switch
        {
            "Common" => Rarity.Common,
            "Uncommon" => Rarity.Uncommon,
            "Rare" => Rarity.Rare,
            "Mythic" => Rarity.MythicRare, // <- the reason why Enum.Parse does not work
            _ => throw new ArgumentException("Unknown Rarity")
        };
    }

    public static MtgArenaAssets Init(string dataDir)
    {
        string downloadsDir = dataDir + "/Downloads";
        Manifest manifest = JsonSerializer.Deserialize<Manifest>(
            File.ReadAllText(GetManifestPath(downloadsDir)))
            ?? throw new Exception("Manifest null");
        Dictionary<ExpansionCodeRarityKey, ExpansionCodeRarityValue> expansionCodeRarityToBundleName = [];
        Dictionary<string, object> bundleLocks = [];

        foreach (var entry in manifest.Assets)
        {
            bundleLocks[entry.Name] = new object();
            foreach (var asset in entry.IndexedAssets)
            {
                if (ExpansionSymbolPathPattern.Match(asset) is { Success: true,
                    Groups: [var _, var code, var rarity] })
                {
                    var key = new ExpansionCodeRarityKey(code.Value, ParseRarity(rarity.Value));
                    expansionCodeRarityToBundleName[key] = new ExpansionCodeRarityValue(entry.Name, asset);
                }
            }
        }

        return new MtgArenaAssets(expansionCodeRarityToBundleName.ToFrozenDictionary(),
            bundleLocks.ToFrozenDictionary(),
            downloadsDir);
    }

    private int GetSpriteIndex(AssetTypeValueField spriteNames, string path)
    {
        int index = 0;
        foreach (var spriteName in spriteNames)
        {
            if (path.EndsWith(spriteName.AsString + ".png"))
            {
                return index;
            }
            index++;
        }
        throw new FileNotFoundException("Sprite not found in Atlas");
    }

    private UnityAssetHelper.TextureData LoadTextureFromBundlePath(string bundleName, string path)
    {
        lock (bundleLocks[bundleName])
        {
            openBundleFiles.TryGetValue(bundleName, out var bundleFile);
            if (bundleFile == null)
            {
                bundleFile = manager.LoadBundleFile($"{downloadsDir}/AssetBundle/{bundleName}", true);
                openBundleFiles[bundleName] = bundleFile;
            }
            var af = manager.LoadAssetsFileFromBundle(bundleFile, 0, false);
            FrozenDictionary<long, string> mappings = LoadPathIdMappings(bundleName, af);

            // sadly not exactly clean, rendering Sprites instead of Textures would be better, but thats way harder
            if (bundleName.StartsWith("Atlas"))
            {
                return LoadFromAtlas(path, af);
            }
            else
            {
                AssetFileInfo? assetInfo = af.file.GetAssetsOfType(AssetClassID.Texture2D)
                    .FirstOrDefault(a => path == mappings[a.PathId])
                        ?? throw new FileNotFoundException($"{path} not found in {bundleName}");
                AssetTypeValueField bf = manager.GetBaseField(af, assetInfo);
                return UnityAssetHelper.DecodeTexture(af, TextureFile.ReadTextureFile(bf));
            }
        }
    }

    private UnityAssetHelper.TextureData LoadFromAtlas(string path, AssetsFileInstance af)
    {
        AssetFileInfo atlasInfo = af.file.GetAssetsOfType(AssetClassID.SpriteAtlas)[0];
        AssetTypeValueField bf = manager.GetBaseField(af, atlasInfo);
        AssetTypeValueField spriteNames = bf["m_PackedSpriteNamesToIndex.Array"];
        int index = GetSpriteIndex(spriteNames, path);

        AssetTypeValueField spriteAtlasData = bf["m_RenderDataMap.Array"][index]["second"];
        var extAsset = manager.GetExtAsset(af, spriteAtlasData["texture"]);
        AssetTypeValueField texBf = manager.GetBaseField(extAsset.file, extAsset.info);
        TextureFile texture = TextureFile.ReadTextureFile(texBf);
        UnityAssetHelper.TextureData texDataUncut = UnityAssetHelper.DecodeTexture(extAsset.file, texture);

        var texRect = spriteAtlasData["textureRect"];
        return texDataUncut with
        {
            Crop = UnityAssetHelper.GetDimensions(texRect)
        };
    }

    private FrozenDictionary<long, string> LoadPathIdMappings(string bundleName, AssetsFileInstance af)
    {
        if (!bundleNameToPathIdMappings.TryGetValue(bundleName, out var mappings))
        {
            var assetBundleInfo = af.file.GetAssetsOfType(AssetClassID.AssetBundle)[0];
            var assetBundleBase = manager.GetBaseField(af, assetBundleInfo);
            mappings = UnityAssetHelper.GetPathIdMappings(assetBundleBase);
            bundleNameToPathIdMappings[bundleName] = mappings;
        }

        return mappings;
    }

    private UnityAssetHelper.TextureData GetExpansionSymbolTexture(string expansionCode, Rarity rarity)
    {
        var key = new ExpansionCodeRarityKey(expansionCode, rarity);
        if (!expansionCodeRarityToBundleName.TryGetValue(key, out ExpansionCodeRarityValue? bundle) || bundle == null)
        {
            throw new ArgumentException($"ExpansionSymbol bundle for {expansionCode} {rarity} not found");
        }

        return LoadTextureFromBundlePath(bundle.BundleName, bundle.Path);
    }

    public byte[] GetExpansionSymbolPng(ExpansionSymbolKey key)
    {
        UnityAssetHelper.TextureData tex = GetExpansionSymbolTexture(key.Code, key.Rarity);
        SKBitmap source = UnityAssetHelper.LoadTextureBitmap(tex);
        UnityAssetHelper.Rect rect = UnityAssetHelper.GetDimensions(tex);

        SKBitmap result = UnityAssetHelper.FlipCrop(source, rect);
        return UnityAssetHelper.AsPng(result);
    }
}

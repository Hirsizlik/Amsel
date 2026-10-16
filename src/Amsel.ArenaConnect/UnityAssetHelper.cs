using System.Collections.Frozen;
using System.Runtime.InteropServices;
using AssetsTools.NET;
using AssetsTools.NET.Extra;
using AssetsTools.NET.Texture;
using SkiaSharp;

namespace Amsel.ArenaConnect;

internal class UnityAssetHelper
{
    internal readonly record struct Rect(int X, int Y, int Width, int Height);
    internal readonly record struct TextureData(byte[] Bgra32, int Width, int Height, Rect? Crop);

    internal static SKBitmap LoadTextureBitmap(TextureData tex)
    {
        var source = new SKBitmap();
        var gcHandle = GCHandle.Alloc(tex.Bgra32, GCHandleType.Pinned);
        var info = new SKImageInfo(tex.Width, tex.Height, SKImageInfo.PlatformColorType, SKAlphaType.Unpremul);
        source.InstallPixels(info, gcHandle.AddrOfPinnedObject(), info.RowBytes, delegate { gcHandle.Free(); });
        return source;
    }

    internal static TextureData DecodeTexture(AssetsFileInstance af, TextureFile texture)
    {
        byte[] raw = texture.FillPictureData(af);
        byte[] decoded = texture.DecodeTextureRaw(raw);
        return new TextureData(decoded, texture.m_Width, texture.m_Height, null);
    }

    internal static SKBitmap FlipCrop(SKBitmap source, Rect crop)
    {
        SKBitmap destination = new(crop.Width, crop.Height);
        using SKCanvas canvas = new(destination);
        canvas.RotateDegrees(180, crop.Width / 2, crop.Height / 2);
        canvas.Translate(crop.Width, 0);
        canvas.Scale(-1, 1);
        canvas.DrawBitmap(source, crop.X, -crop.Y, SKSamplingOptions.Default);
        return destination;
    }

    internal static Rect GetDimensions(TextureData tex)
    {
        return tex.Crop is { } c 
            ? c 
            : new Rect(0, 0, tex.Width, tex.Height);
    }

    internal static Rect GetDimensions(AssetTypeValueField texRect)
    {
        return new Rect(
            texRect["x"].AsInt,
            texRect["y"].AsInt,
            texRect["width"].AsInt,
            texRect["height"].AsInt
        );
    }

    internal static byte[] AsPng(SKBitmap bitmap)
    {
        SKImage image = SKImage.FromBitmap(bitmap);
        return image.Encode(SKEncodedImageFormat.Png, 100).ToArray();
    }

    internal static FrozenDictionary<long, string> GetPathIdMappings(AssetTypeValueField assetBundleBase)
    {
        Dictionary<long, string> temp = [];
        var abContainer = assetBundleBase["m_Container.Array"];
        foreach (var data in abContainer.Children)
        {
            var name = data[0].AsString;
            var pathId = data[1]["asset.m_PathID"].AsLong;
            temp[pathId] = name;
        }
        return temp.ToFrozenDictionary();
    }
}

using Amsel.Data;

namespace Amsel.ArenaConnect.Test;

public class MtgArenaAssetsTest
{
    private IMtgArenaAssets aa;

    [OneTimeSetUp]
    public void Setup()
    {
        MtgArenaConnect connect = new();
        string datadir = connect.GetDataDirPath();
        aa = MtgArenaAssets.Init(datadir);
    }

    [Test]
    public void TestGetExpansionSymbol()
    {
        var t1 = aa.GetExpansionSymbol("WOE", Rarity.Common);
        Assert.That(t1.Bgra32, Is.Not.Empty);
        Assert.That(t1.Crop, Is.Null);
    }

    [Test]
    public void TestGetExpansionSymbolAtlas()
    {
        // MID and PRM have different logic...
        var t1 = aa.GetExpansionSymbol("MID", Rarity.Common);
        Assert.That(t1.Bgra32, Is.Not.Empty);
        Assert.That(t1.Crop, Is.Not.Null);
    }
}

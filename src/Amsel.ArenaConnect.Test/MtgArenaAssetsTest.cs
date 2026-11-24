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
        var t1 = aa.GetExpansionSymbolPng(new ExpansionSymbolKey("WOE", Rarity.Common));
        Assert.That(t1, Is.Not.Empty);
    }

    [Test]
    public void TestGetExpansionSymbolAtlas()
    {
        // MID and PRM have different logic...
        var t1 = aa.GetExpansionSymbolPng(new ExpansionSymbolKey("MID", Rarity.Common));
        Assert.That(t1, Is.Not.Empty);
    }
}

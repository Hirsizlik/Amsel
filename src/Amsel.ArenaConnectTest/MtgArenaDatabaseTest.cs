using Amsel.ArenaConnect;

namespace Amsel.ArenaConnectTest;

public class MtgArenaDatabaseTest
{
    private IMtgArenaDatabase mad;

    [OneTimeSetUp]
    public void Setup()
    {
        MtgArenaConnect connect = new();
        mad = new MtgArenaDatabase(connect.GetDatabasePath());
    }

    [OneTimeTearDown]
    public void TearDown()
    {
        mad.Dispose();
    }

    [Test]
    public void TestGetAllCards()
    {
        var locs = mad.GetEnglishLocalization();
        Assert.That(locs, Is.Not.Empty);
        var cards = mad.GetAllCards(locs);
        Assert.That(cards, Is.Not.Empty);
    }
}

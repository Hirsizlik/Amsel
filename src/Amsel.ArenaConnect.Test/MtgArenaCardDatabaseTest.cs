namespace Amsel.ArenaConnect.Test;

public class MtgArenaCardDatabaseTest
{
    private IMtgArenaCardDatabase mad;

    [OneTimeSetUp]
    public void Setup()
    {
        MtgArenaConnect connect = new();
        mad = new MtgArenaCardDatabase(connect.GetCardDatabasePath());
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

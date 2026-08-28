namespace Amsel.ArenaConnect.Test;

public class MtgArenaClientLocalizationDatabaseTest
{
    private IMtgArenaClientLocalizationDatabase macld;

    [OneTimeSetUp]
    public void Setup()
    {
        MtgArenaConnect connect = new();
        macld = new MtgArenaClientLocalizationDatabase(connect.GetClientLocalizationDatabasePath());
    }

    [OneTimeTearDown]
    public void TearDown()
    {
        macld.Dispose();
    }

    [Test]
    public void TestGetAllCards()
    {
        var locs = macld.GetEnglishSetLocalization();
        Assert.That(locs, Is.Not.Empty);
    }
}

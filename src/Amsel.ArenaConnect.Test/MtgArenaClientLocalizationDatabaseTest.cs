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
    public void TestGetEnglishSetLocalization()
    {
        var locs = macld.GetEnglishSetLocalization();
        Assert.That(locs, Is.Not.Empty);
    }

    [Test]
    public void TestGetEnglishFormatLocalization()
    {
        var locs = macld.GetEnglishFormatLocalization();
        Assert.That(locs, Is.Not.Empty);
    }
}

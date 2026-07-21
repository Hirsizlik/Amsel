namespace Amsel.ArenaConnectTest;

using Amsel.ArenaConnect;

public class MtgArenaConnectTest
{
    private MtgArenaConnect mac;

    [OneTimeSetUp]
    public void Init()
    {
        mac = new();
    }

    [Test]
    public void TestGetCardsOwnedFromInventory()
    {
        var c = mac.GetCardsOwnedFromInventory();
        Assert.That(c, Is.Not.Empty);
    }

    [Test]
    public void TestGetConnectionString()
    {
        var s = mac.GetDatabasePath();
        Assert.That(mac.GetDatabasePath(), Is.Not.Empty);
    }
}

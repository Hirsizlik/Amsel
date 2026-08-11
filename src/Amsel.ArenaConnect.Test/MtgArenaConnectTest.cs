namespace Amsel.ArenaConnect.Test;

public class MtgArenaConnectTest
{
    private IMtgArenaConnect mac;

    [OneTimeSetUp]
    public void Init()
    {
        mac = new MtgArenaConnect();
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

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
    public void TestGetCardDatabasePath()
    {
        var s = mac.GetCardDatabasePath();
        Assert.That(s, Is.Not.Empty);
    }

    [Test]
    public void TestGetSetMetadata()
    {
        var s = mac.GetSetMetadata();
        Assert.That(s, Is.Not.Empty);
    }
}

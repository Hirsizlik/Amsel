namespace Amsel.Data.Test;

public class TextSearchTest
{

    private static CardStats WithName(string name)
    {
        return new CardStats(new CardInfo(0, name, "", "", 0, null, Rarity.Unknown, true), 0);
    }

    private static CardStats WithRarity(Rarity rarity)
    {
        return new CardStats(new CardInfo(0, "", "", "", 0, null, rarity, true), 0);
    }

    private static CardStats WithQuantity(int quantity)
    {
        return new CardStats(new CardInfo(0, "", "", "", 0, null, Rarity.Unknown, true), quantity);
    }

    [Test]
    public void TestNameSingleWord()
    {
        Assert.That(TextSearch.TryParse("Test", out TextSearch ts), Is.True);
        using (Assert.EnterMultipleScope())
        {
            Assert.That(ts.FilterCard(WithName("my little test")), Is.True);
            Assert.That(ts.FilterCard(WithName("MY BIG TEST")), Is.True);
            Assert.That(ts.FilterCard(WithName("nope no te_st")), Is.False);
        }
    }

    [Test]
    public void TestNameInQuotes()
    {
        Assert.That(TextSearch.TryParse("\"My Test\"", out TextSearch ts), Is.True);
        using (Assert.EnterMultipleScope())
        {
            Assert.That(ts.FilterCard(WithName("my test")), Is.True);
            Assert.That(ts.FilterCard(WithName("my other test")), Is.False);
            Assert.That(ts.FilterCard(WithName("my testing")), Is.True);
        }
    }

    [Test]
    public void TestNameBrokenQuotes()
    {
        Assert.That(TextSearch.TryParse("\"My Test", out TextSearch ts), Is.False);
    }

    [Test]
    public void TestNameMultipeWords()
    {
        Assert.That(TextSearch.TryParse("My Test", out TextSearch ts), Is.True);
        using (Assert.EnterMultipleScope())
        {
            Assert.That(ts.FilterCard(WithName("my test")), Is.True);
            Assert.That(ts.FilterCard(WithName("testing myriads")), Is.True);
            Assert.That(ts.FilterCard(WithName("your test")), Is.False);
        }
    }

    private static void AssertRarity(string search, IEnumerable<Rarity> hitList)
    {
        Assert.That(TextSearch.TryParse(search, out TextSearch ts), Is.True);
        using (Assert.EnterMultipleScope())
        {
            foreach (Rarity r in hitList)
            {
                Assert.That(ts.FilterCard(WithRarity(r)), Is.True, r.ToString());
            }
            foreach (Rarity r in Enum.GetValues<Rarity>().Except(hitList))
            {
                Assert.That(ts.FilterCard(WithRarity(r)), Is.False, r.ToString());
            }
        }
    }

    [Test]
    public void TestRarityEqual()
    {
        AssertRarity("r=L", [Rarity.Land]);
        AssertRarity("r=C", [Rarity.Common]);
        AssertRarity("r=U", [Rarity.Uncommon]);
        AssertRarity("r=R", [Rarity.Rare]);
        AssertRarity("r=M", [Rarity.MythicRare]);
    }

    [Test]
    public void TestRarityGreater()
    {
        AssertRarity("r>U", [Rarity.Rare, Rarity.MythicRare]);
    }

    [Test]
    public void TestRarityGreaterEqual()
    {
        AssertRarity("r>=R", [Rarity.Rare, Rarity.MythicRare]);
    }

    [Test]
    public void TestRarityLesser()
    {
        AssertRarity("r<U", [Rarity.Unknown, Rarity.Land, Rarity.Common]);
    }

    [Test]
    public void TestRarityLesserEqual()
    {
        AssertRarity("r<=C", [Rarity.Unknown, Rarity.Land, Rarity.Common]);
    }

    [Test]
    public void TestRarityNotEqual()
    {
        AssertRarity("r!=U", [Rarity.Unknown, Rarity.Land, Rarity.Common, Rarity.Rare, Rarity.MythicRare]);
    }

    private static void AssertQuantiy(string search, bool zero, bool two, bool four)
    {
        Assert.That(TextSearch.TryParse(search, out TextSearch ts), Is.True);
        using (Assert.EnterMultipleScope())
        {
            Assert.That(ts.FilterCard(WithQuantity(0)), zero ? Is.True : Is.False);
            Assert.That(ts.FilterCard(WithQuantity(2)), two ? Is.True : Is.False);
            Assert.That(ts.FilterCard(WithQuantity(4)), four ? Is.True : Is.False);
        }
    }

    [Test]
    public void TestQuantityEqual()
    {
        AssertQuantiy("q=2", false, true, false);
    }

    [Test]
    public void TestQuantityNotEqual()
    {
        AssertQuantiy("q!=2", true, false, true);
    }

    [Test]
    public void TestQuantityGreater()
    {
        AssertQuantiy("q>2", false, false, true);
    }

    [Test]
    public void TestQuantityGreaterEqual()
    {
        AssertQuantiy("q>=2", false, true, true);
    }

    [Test]
    public void TestQuantityLesser()
    {
        AssertQuantiy("q<2", true, false, false);
    }

    [Test]
    public void TestQuantityLesserEqual()
    {
        AssertQuantiy("q<=2", true, true, false);
    }

    [Test]
    public void TestComboFilter()
    {
        Assert.That(TextSearch.TryParse("my r>U q>2 test", out TextSearch ts), Is.True);
        using (Assert.EnterMultipleScope())
        {
            Assert.That(ts.FilterCard(
                new CardStats(new CardInfo(0, "testing mythic rare myr", "", "", 0, null, Rarity.MythicRare, true), 3)),
                Is.True);
            Assert.That(ts.FilterCard(
                new CardStats(new CardInfo(0, "testing common myr", "", "", 0, null, Rarity.Common, true), 3)),
                Is.False);
            Assert.That(ts.FilterCard(
                new CardStats(new CardInfo(0, "testing not enough myr", "", "", 0, null, Rarity.Common, true), 2)),
                Is.False);
            Assert.That(ts.FilterCard(
                new CardStats(new CardInfo(0, "test frog", "", "", 0, null, Rarity.Rare, true), 4)),
                Is.False);
        }
    }

    [Test]
    public void TestEquals()
    {
        Assume.That(TextSearch.TryParse("my r>U q>2 test", out TextSearch tsOrig), Is.True);
        Assume.That(TextSearch.TryParse("q>2 r>U test my", out TextSearch tsOtherOrder), Is.True);
        Assume.That(TextSearch.TryParse("MY R>U Q>2 TEST", out TextSearch tsBlockCase), Is.True);
        Assume.That(TextSearch.TryParse("q>=2 r>=U myr test", out TextSearch tsNotEqual1), Is.True);
        Assume.That(TextSearch.TryParse("my test", out TextSearch tsNotEqual2), Is.True);
        Assume.That(TextSearch.TryParse("my r>U q>2 test too", out TextSearch tsNotEqual3), Is.True);
        Assume.That(TextSearch.TryParse("", out TextSearch tsEmpty), Is.True);
        using (Assert.EnterMultipleScope())
        {
            Assert.That(tsOrig, Is.EqualTo(tsOtherOrder));
            Assert.That(tsOrig, Is.EqualTo(tsBlockCase));
            Assert.That(tsOrig, Is.Not.EqualTo(tsNotEqual1));
            Assert.That(tsOrig, Is.Not.EqualTo(tsNotEqual2));
            Assert.That(tsOrig, Is.Not.EqualTo(tsNotEqual3));
            Assert.That(tsOrig, Is.Not.EqualTo(tsEmpty));
        }
    }
}

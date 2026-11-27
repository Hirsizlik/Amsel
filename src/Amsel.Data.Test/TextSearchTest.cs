using System.Collections.Frozen;
using System.Collections.Immutable;

namespace Amsel.Data.Test;

public class TextSearchTest
{

    private static readonly FrozenDictionary<uint, int> o = FrozenDictionary.Create<uint, int>([]);

    private static CardStats WithName(string name)
    {
        return new CardStats(new CardInfo(0, name, "", "", 0, null, Rarity.Unknown, true, 0, []), 0, true);
    }

    private static CardStats WithRarity(Rarity rarity)
    {
        return new CardStats(new CardInfo(0, "", "", "", 0, null, rarity, true, 0, []), 0, true);
    }

    private static CardStats WithQuantity(int quantity)
    {
        return new CardStats(new CardInfo(0, "", "", "", 0, null, Rarity.Unknown, true, 0, []), quantity, true);
    }

    [Test]
    public void TestNameSingleWord()
    {
        Assert.That(TextSearch.TryParse("Test", [], o, out TextSearch ts), Is.True);
        using (Assert.EnterMultipleScope())
        {
            Assert.That(ts.FilterCard(WithName("my little test")), Is.True);
            Assert.That(ts.FilterCard(WithName("MY BIG TEST")), Is.True);
            Assert.That(ts.FilterCard(WithName("nope no te_st")), Is.False);
        }
    }

    [Test]
    public void TestNameWithColon()
    {
        Assert.That(TextSearch.TryParse("Protection:", [], o, out TextSearch ts1), Is.True);
        Assert.That(TextSearch.TryParse(":", [], o, out TextSearch ts2), Is.True);
        var c = WithName("Circle of Protection: Black");
        using (Assert.EnterMultipleScope())
        {
            Assert.That(ts1.FilterCard(c), Is.True);
            Assert.That(ts2.FilterCard(c), Is.True);
        }
    }

    [Test]
    public void TestNameInQuotes()
    {
        Assert.That(TextSearch.TryParse("\"My Test\"", [], o, out TextSearch ts), Is.True);
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
        Assert.That(TextSearch.TryParse("\"My Test", [], o, out TextSearch ts), Is.False);
    }

    [Test]
    public void TestNameMultipeWords()
    {
        Assert.That(TextSearch.TryParse("My Test", [], o, out TextSearch ts), Is.True);
        using (Assert.EnterMultipleScope())
        {
            Assert.That(ts.FilterCard(WithName("my test")), Is.True);
            Assert.That(ts.FilterCard(WithName("testing myriads")), Is.True);
            Assert.That(ts.FilterCard(WithName("your test")), Is.False);
        }
    }

    private static void AssertRarity(string search, IEnumerable<Rarity> hitList)
    {
        Assert.That(TextSearch.TryParse(search, [], o, out TextSearch ts), Is.True);
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

        AssertRarity("r:L", [Rarity.Land]);
        AssertRarity("r:C", [Rarity.Common]);
        AssertRarity("r:U", [Rarity.Uncommon]);
        AssertRarity("r:R", [Rarity.Rare]);
        AssertRarity("r:M", [Rarity.MythicRare]);
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
        AssertRarity("r<r", [Rarity.Unknown, Rarity.Land, Rarity.Common, Rarity.Uncommon]);
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
        Assert.That(TextSearch.TryParse(search, [], o, out TextSearch ts), Is.True);
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
        AssertQuantiy("q:2", false, true, false);
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
    public void TestQuantityOverall()
    {
        FrozenDictionary<uint, int> dict = FrozenDictionary.Create(KeyValuePair.Create<uint, int>(100, 8));
        Assert.That(TextSearch.TryParse("qo>0", [], dict, out TextSearch t1), Is.True);
        Assert.That(TextSearch.TryParse("qo<=4", [], dict, out TextSearch t2), Is.True);

        using (Assert.EnterMultipleScope())
        {
            Assert.That(t1.FilterCard(new CardStats(
                new CardInfo(0, "", "", "", 0, null, Rarity.Unknown, true, 100, []), 0, false)), Is.True);
            Assert.That(t2.FilterCard(new CardStats(
                new CardInfo(0, "", "", "", 0, null, Rarity.Unknown, true, 100, []), 0, false)), Is.False);
        }
    }

    [Test]
    public void TestComboFilter()
    {
        Assert.That(TextSearch.TryParse("my r>U q>2 test", [], o, out TextSearch ts), Is.True);
        using (Assert.EnterMultipleScope())
        {
            Assert.That(ts.FilterCard(
                new CardStats(new CardInfo(0, "testing mythic rare myr", "", "", 0, null,
                Rarity.MythicRare, true, 1, []), 3, true)),
                Is.True);
            Assert.That(ts.FilterCard(
                new CardStats(new CardInfo(0, "testing common myr", "", "", 0, null,
                Rarity.Common, true, 2, []), 3, true)),
                Is.False);
            Assert.That(ts.FilterCard(
                new CardStats(new CardInfo(0, "testing not enough myr", "", "", 0, null,
                Rarity.Common, true, 3, []), 2, true)),
                Is.False);
            Assert.That(ts.FilterCard(
                new CardStats(new CardInfo(0, "test frog", "", "", 0, null,
                Rarity.Rare, true, 4, []), 4, true)),
                Is.False);
        }
    }

    [Test]
    public void TestEquals()
    {
        Assume.That(TextSearch.TryParse("my r>U q>2 test", [], o, out TextSearch tsOrig), Is.True);
        Assume.That(TextSearch.TryParse("q>2 r>U test my", [], o, out TextSearch tsOtherOrder), Is.True);
        Assume.That(TextSearch.TryParse("MY R>U Q>2 TEST", [], o, out TextSearch tsBlockCase), Is.True);
        Assume.That(TextSearch.TryParse("q>=2 r>=U myr test", [], o, out TextSearch tsNotEqual1), Is.True);
        Assume.That(TextSearch.TryParse("my test", [], o, out TextSearch tsNotEqual2), Is.True);
        Assume.That(TextSearch.TryParse("my r>U q>2 test too", [], o, out TextSearch tsNotEqual3), Is.True);
        Assume.That(TextSearch.TryParse("", [], o, out TextSearch tsEmpty), Is.True);
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

    [Test]
    public void TestInvalid()
    {
        Assert.That(TextSearch.TryParse("x=1", [], o, out TextSearch _), Is.False);
        Assert.That(TextSearch.TryParse("xxx:1", [], o, out TextSearch _), Is.False);
        Assert.That(TextSearch.TryParse("q:U", [], o, out TextSearch _), Is.False);
        Assert.That(TextSearch.TryParse("qq:3", [], o, out TextSearch _), Is.False);
        Assert.That(TextSearch.TryParse("r<7", [], o, out TextSearch _), Is.False);
        Assert.That(TextSearch.TryParse("f=Standard", [], o, out TextSearch _), Is.False);
        Assert.That(TextSearch.TryParse("is<banned", [], o, out TextSearch _), Is.False);
        Assert.That(TextSearch.TryParse("is:invalid", [], o, out TextSearch _), Is.False);
    }

    [Test]
    public void TestFormat()
    {
        ImmutableArray<FormatInformation> format = [new FormatInformation(new FormatData("Ignored by filter", [], [2],
            FrozenDictionary.Create([KeyValuePair.Create<uint, Quota>(5, new Quota(1))]), [4], [1, 5]), "Format")];
        Assert.That(TextSearch.TryParse("f=Format", format, o, out TextSearch tsInFormat), Is.True);
        Assert.That(TextSearch.TryParse("f!=Format", format, o, out TextSearch tsNotInFormat), Is.True);
        Assert.That(TextSearch.TryParse("legal=Format", format, o, out TextSearch tsLegal), Is.True);
        Assert.That(TextSearch.TryParse("legal!=Format", format, o, out TextSearch tsNotLegal), Is.True);
        Assert.That(TextSearch.TryParse("banned=Format", format, o, out TextSearch tsBanned), Is.True);
        Assert.That(TextSearch.TryParse("banned!=Format", format, o, out TextSearch tsNotBanned), Is.True);
        using (Assert.EnterMultipleScope())
        {
            // legal
            Assert.That(tsInFormat.FilterCard(
                new CardStats(new CardInfo(0, "", "", "", 0, null, Rarity.MythicRare, true, 1, []), 3, true)),
                Is.True);
            Assert.That(tsNotInFormat.FilterCard(
                new CardStats(new CardInfo(0, "", "", "", 0, null, Rarity.MythicRare, true, 1, []), 3, true)),
                Is.False);

            Assert.That(tsLegal.FilterCard(
                new CardStats(new CardInfo(0, "", "", "", 0, null, Rarity.MythicRare, true, 1, []), 3, true)),
                Is.True);
            Assert.That(tsNotLegal.FilterCard(
                new CardStats(new CardInfo(0, "", "", "", 0, null, Rarity.MythicRare, true, 1, []), 3, true)),
                Is.False);

            Assert.That(tsBanned.FilterCard(
                new CardStats(new CardInfo(0, "", "", "", 0, null, Rarity.MythicRare, true, 1, []), 3, true)),
                Is.False);
            Assert.That(tsNotBanned.FilterCard(
                new CardStats(new CardInfo(0, "", "", "", 0, null, Rarity.MythicRare, true, 1, []), 3, true)),
                Is.True);

            // banned
            Assert.That(tsInFormat.FilterCard(
                new CardStats(new CardInfo(0, "", "", "", 0, null, Rarity.MythicRare, true, 2, []), 3, true)),
                Is.True);
            Assert.That(tsNotInFormat.FilterCard(
                new CardStats(new CardInfo(0, "", "", "", 0, null, Rarity.MythicRare, true, 2, []), 3, true)),
                Is.False);

            Assert.That(tsLegal.FilterCard(
                new CardStats(new CardInfo(0, "", "", "", 0, null, Rarity.MythicRare, true, 2, []), 3, true)),
                Is.False);
            Assert.That(tsNotLegal.FilterCard(
                new CardStats(new CardInfo(0, "", "", "", 0, null, Rarity.MythicRare, true, 2, []), 3, true)),
                Is.True);

            Assert.That(tsBanned.FilterCard(
                new CardStats(new CardInfo(0, "", "", "", 0, null, Rarity.MythicRare, true, 2, []), 3, true)),
                Is.True);
            Assert.That(tsNotBanned.FilterCard(
                new CardStats(new CardInfo(0, "", "", "", 0, null, Rarity.MythicRare, true, 2, []), 3, true)),
                Is.False);

            // not in format
            Assert.That(tsInFormat.FilterCard(
                new CardStats(new CardInfo(0, "", "", "", 0, null, Rarity.MythicRare, true, 3, []), 3, true)),
                Is.False);
            Assert.That(tsNotInFormat.FilterCard(
                new CardStats(new CardInfo(0, "", "", "", 0, null, Rarity.MythicRare, true, 3, []), 3, true)),
                Is.True);

            Assert.That(tsLegal.FilterCard(
                new CardStats(new CardInfo(0, "", "", "", 0, null, Rarity.MythicRare, true, 3, []), 3, true)),
                Is.False);
            Assert.That(tsNotLegal.FilterCard(
                new CardStats(new CardInfo(0, "", "", "", 0, null, Rarity.MythicRare, true, 3, []), 3, true)),
                Is.True);

            Assert.That(tsBanned.FilterCard(
                new CardStats(new CardInfo(0, "", "", "", 0, null, Rarity.MythicRare, true, 3, []), 3, true)),
                Is.False);
            Assert.That(tsNotBanned.FilterCard(
                new CardStats(new CardInfo(0, "", "", "", 0, null, Rarity.MythicRare, true, 3, []), 3, true)),
                Is.True);
        }
    }

    [Test]
    public void TestMultiWordFormat()
    {
        ImmutableArray<FormatInformation> format = [new FormatInformation(new FormatData("Ignored", [], [2],
            FrozenDictionary.Create([KeyValuePair.Create<uint, Quota>(5, new Quota(1))]), [4], [1, 5]), "Standard Brawl")];
        Assert.That(TextSearch.TryParse("\"f=Standard Brawl\"", format, o, out TextSearch _), Is.True);
        Assert.That(TextSearch.TryParse("\"f=Standard\"", format, o, out TextSearch _), Is.False);
        // f=Standard plus "Brawl" in name
        Assert.That(TextSearch.TryParse("f=Standard Brawl", format, o, out TextSearch _), Is.False);
    }

    [Test]
    public void TestIsBannedFilter()
    {
        ImmutableArray<FormatInformation> format = [new FormatInformation(new FormatData("name", [], [2],
            FrozenDictionary.Create([KeyValuePair.Create<uint, Quota>(5, new Quota(1))]), [4], [1, 5]), "name")];

        Assert.That(TextSearch.TryParse("is=banned", format, o, out TextSearch tsBanned), Is.True);
        Assert.That(TextSearch.TryParse("is!=banned", format, o, out TextSearch tsNotBanned), Is.True);
        using (Assert.EnterMultipleScope())
        {
            // legal
            Assert.That(tsNotBanned.FilterCard(
                new CardStats(new CardInfo(0, "", "", "", 0, null, Rarity.MythicRare, true, 1, []), 3, true)),
                Is.True);
            Assert.That(tsBanned.FilterCard(
                new CardStats(new CardInfo(0, "", "", "", 0, null, Rarity.MythicRare, true, 1, []), 3, true)),
                Is.False);

            // banned
            Assert.That(tsNotBanned.FilterCard(
                new CardStats(new CardInfo(0, "", "", "", 0, null, Rarity.MythicRare, true, 2, []), 3, true)),
                Is.False);
            Assert.That(tsBanned.FilterCard(
                new CardStats(new CardInfo(0, "", "", "", 0, null, Rarity.MythicRare, true, 2, []), 3, true)),
                Is.True);

            // not in legal list (not banned)
            Assert.That(tsNotBanned.FilterCard(
                new CardStats(new CardInfo(0, "", "", "", 0, null, Rarity.MythicRare, true, 3, []), 3, true)),
                Is.True);
            Assert.That(tsBanned.FilterCard(
                new CardStats(new CardInfo(0, "", "", "", 0, null, Rarity.MythicRare, true, 3, []), 3, true)),
                Is.False);

            // banned as Commander (same as banned)
            Assert.That(tsNotBanned.FilterCard(
                new CardStats(new CardInfo(0, "", "", "", 0, null, Rarity.MythicRare, true, 4, []), 3, true)),
                Is.False);
            Assert.That(tsBanned.FilterCard(
                new CardStats(new CardInfo(0, "", "", "", 0, null, Rarity.MythicRare, true, 4, []), 3, true)),
                Is.True);

            // restricted (same as banned)
            Assert.That(tsNotBanned.FilterCard(
                new CardStats(new CardInfo(0, "", "", "", 0, null, Rarity.MythicRare, true, 5, []), 3, true)),
                Is.False);
            Assert.That(tsBanned.FilterCard(
                new CardStats(new CardInfo(0, "", "", "", 0, null, Rarity.MythicRare, true, 5, []), 3, true)),
               Is.True);
        }
    }

    [Test]
    public void TestIsCollectible()
    {
        Assert.That(TextSearch.TryParse("is=collectible", [], o, out TextSearch tsCollectible), Is.True);
        Assert.That(TextSearch.TryParse("Is!=Collectible", [], o, out TextSearch tsNotCollectible), Is.True);
        using (Assert.EnterMultipleScope())
        {
            // collectible
            Assert.That(tsCollectible.FilterCard(
                new CardStats(new CardInfo(0, "", "", "", 0, null, Rarity.MythicRare, true, 1, []), 3, true)),
                Is.True);
            Assert.That(tsNotCollectible.FilterCard(
                new CardStats(new CardInfo(0, "", "", "", 0, null, Rarity.MythicRare, true, 1, []), 3, true)),
                Is.False);

            // not collectible
            Assert.That(tsCollectible.FilterCard(
                new CardStats(new CardInfo(0, "", "", "", 0, null, Rarity.MythicRare, true, 1, []), 3, false)),
                Is.False);
            Assert.That(tsNotCollectible.FilterCard(
                new CardStats(new CardInfo(0, "", "", "", 0, null, Rarity.MythicRare, true, 1, []), 3, false)),
                Is.True);
        }
    }
}

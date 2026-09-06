namespace Amsel.Data.Test;

public class SetStatisticTest
{

    private static void AssertRarity(SetStatistic actual, Rarity r, int expTotal, int expUnique, int expCardCount)
    {
        Assert.That(actual.GetAmountTotal(r), Is.EqualTo(expTotal),
            $"Total {actual.GetAmountTotal(r)} {r} {expTotal}");
        Assert.That(actual.GetAmountUnique(r), Is.EqualTo(expUnique),
            $"Unique {actual.GetAmountUnique(r)} {r} {expUnique}");
        Assert.That(actual.GetCardCount(r), Is.EqualTo(expCardCount),
            $"CardCount {actual.GetCardCount(r)} {r} {expCardCount}");
    }

    private static void AssertSetAndTotal(SetStatistic actual, int expTotal, int expUnique,
        int expCardCount)
    {
        Assert.That(actual.TotalOwned, Is.EqualTo(expTotal), $"Total {actual.TotalOwned} {expTotal}");
        Assert.That(actual.TotalOwnedUnique, Is.EqualTo(expUnique), $"Unique {actual.TotalOwnedUnique} {expUnique}");
        Assert.That(actual.CardCount, Is.EqualTo(expCardCount), $"CardCount {actual.CardCount} {expCardCount}");
    }

    [Test]
    public void TestSingleSet()
    {
        var actualStatisticDict = SetStatistic.CreateStatistics([
            new CardStats(new CardInfo(1, "Card 1", "TEST", "DTEST", 1, 4, Rarity.Common, true, 1, []), 1),
            new CardStats(new CardInfo(2, "Card 2", "TEST", "DTEST", 2, 4, Rarity.Common, true, 2, []), 2),
            new CardStats(new CardInfo(3, "Card 3", "TEST", "DTEST", 3, 4, Rarity.Rare, true, 3, []), 3),
            new CardStats(new CardInfo(4, "Card 4", "TEST", "DTEST", 4, 4, Rarity.MythicRare, true, 4, []), 0),
            new CardStats(new CardInfo(5, "Card Bonus", "TEST", "DTEST", 4, null, Rarity.MythicRare, true, 5, []), 4),
        ]);
        Assert.That(actualStatisticDict.Count, Is.EqualTo(2));
        Span<string> expectedSets = ["TEST", "DTEST"];
        using (Assert.EnterMultipleScope())
        {
            foreach (string s in expectedSets)
            {
                var actual = actualStatisticDict[s];
                AssertSetAndTotal(actual, 10, 4, 5);
                AssertRarity(actual, Rarity.Common, 3, 2, 2);
                AssertRarity(actual, Rarity.Uncommon, 0, 0, 0);
                AssertRarity(actual, Rarity.Rare, 3, 1, 1);
                AssertRarity(actual, Rarity.MythicRare, 4, 1, 2);
            }
        }
    }

    [Test]
    public void TestMultipleSets()
    {
        var actualStatisticDict = SetStatistic.CreateStatistics([
            new CardStats(new CardInfo(1, "Card 1", "T1", "D1", 1, 1, Rarity.Common, true, 1, []), 1),
            new CardStats(new CardInfo(2, "Card 2", "T2", "D1", 2, 3, Rarity.Common, true, 1, []), 2),
            new CardStats(new CardInfo(3, "Card 3", "T2", "D2", 3, null, Rarity.Common, true, 1, []), 3),
        ]);
        Assert.That(actualStatisticDict.Count, Is.EqualTo(4));
        var actualT1 = actualStatisticDict["T1"];
        var actualT2 = actualStatisticDict["T2"];
        var actualD1 = actualStatisticDict["D1"];
        var actualD2 = actualStatisticDict["D2"];

        using (Assert.EnterMultipleScope())
        {
            AssertSetAndTotal(actualT1, 1, 1, 1);
            AssertRarity(actualT1, Rarity.Common, 1, 1, 1);

            AssertSetAndTotal(actualT2, 5, 2, 2);
            AssertRarity(actualT2, Rarity.Common, 5, 2, 2);

            AssertSetAndTotal(actualD1, 3, 2, 2);
            AssertRarity(actualD1, Rarity.Common, 3, 2, 2);

            AssertSetAndTotal(actualD2, 3, 1, 1);
            AssertRarity(actualD2, Rarity.Common, 3, 1, 1);
        }
    }
}

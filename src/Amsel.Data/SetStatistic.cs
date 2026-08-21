using System.Collections.Immutable;

namespace Amsel.Data;

public class SetStatistic
{
    public string SetCode { get; init; }
    public bool IsDigitalSet { get; init; }
    private readonly Dictionary<Rarity, int> amountByRarity = CreateEmptyDictWithRarity();
    private readonly Dictionary<Rarity, int> amountByRarityUnique = CreateEmptyDictWithRarity();
    private readonly Dictionary<Rarity, int> cardCountByRarity = CreateEmptyDictWithRarity();
    public int TotalOwned { get; private set; } = 0;
    public int TotalOwnedUnique { get; private set; } = 0;
    public int CardCount { get; private set; } = 0; // never 0 after CreateStatistics, empty sets aren't included

    public int GetAmountTotal(Rarity r) => amountByRarity[r];
    public int GetAmountUnique(Rarity r) => amountByRarityUnique[r];
    public int GetCardCount(Rarity r) => cardCountByRarity[r];

    private SetStatistic(string setCode, bool isDigitalSet)
    {
        SetCode = setCode;
        IsDigitalSet = isDigitalSet;
    }

    private static Dictionary<Rarity, int> CreateEmptyDictWithRarity()
    {
        return new Dictionary<Rarity, int>
        {
            [Rarity.Unknown] = 0,
            [Rarity.Land] = 0,
            [Rarity.Common] = 0,
            [Rarity.Uncommon] = 0,
            [Rarity.Rare] = 0,
            [Rarity.MythicRare] = 0,
        };
    }

    public static Dictionary<string, SetStatistic> CreateStatistics(ImmutableArray<CardStats> cards)
    {
        Dictionary<string, SetStatistic> result = [];
        foreach (CardStats c in cards)
        {
            AddToSetStatistic(c.Info.ExpansionCode, false, c, result);
            AddToSetStatistic(c.Info.DigitalReleaseSet, true, c, result);
        }
        return result;
    }

    private static void AddToSetStatistic(string code, bool isDigital, CardStats cs,
        Dictionary<string, SetStatistic> result)
    {
        if (string.IsNullOrEmpty(code))
            return;
        if (!result.TryGetValue(code, out SetStatistic? current))
        {
            current = new SetStatistic(code, isDigital);
            result.Add(code, current);
        }
        int zeroOrOne = cs.Owned > 0 ? 1 : 0;
        current.amountByRarity[cs.Info.Rarity] += cs.Owned;
        current.amountByRarityUnique[cs.Info.Rarity] += zeroOrOne;
        current.cardCountByRarity[cs.Info.Rarity] += 1;

        current.TotalOwned += cs.Owned;
        current.TotalOwnedUnique += zeroOrOne;
        current.CardCount += 1;
    }
}

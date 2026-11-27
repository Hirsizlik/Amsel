using System.Collections.Immutable;

namespace Amsel.Data;

public class SetStatistic
{
    private readonly Dictionary<Rarity, int> amountByRarity = CreateEmptyDictWithRarity();
    private readonly Dictionary<Rarity, int> amountByRarityUnique = CreateEmptyDictWithRarity();
    private readonly Dictionary<Rarity, int> cardCountByRarity = CreateEmptyDictWithRarity();
    public int TotalOwned { get; private set; } = 0;
    public int TotalOwnedUnique { get; private set; } = 0;
    public int CardCount { get; private set; } = 0; // never 0 after CreateStatistics, empty sets aren't included
    public double PercentOwnedUnique { get => 100.0 * TotalOwnedUnique / CardCount; }
    public double PercentOwned { get => 25.0 * TotalOwned / CardCount; }

    public int GetAmountTotal(Rarity r) => amountByRarity[r];
    public int GetAmountUnique(Rarity r) => amountByRarityUnique[r];
    public int GetCardCount(Rarity r) => cardCountByRarity[r];
    public double GetPercentTotal(Rarity r) => 25.0 * GetAmountTotal(r) / GetCardCount(r);
    public double GetPercentUnique(Rarity r) => 100.0 * GetAmountUnique(r) / GetCardCount(r);

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
            foreach (var setCode in ImmutableArray.Create([c.Info.ExpansionCode, c.Info.DigitalReleaseSet]))
            {
                if (string.IsNullOrWhiteSpace(setCode))
                    continue;
                if (!result.TryGetValue(setCode, out SetStatistic? current)) {
                    current = new SetStatistic();
                    result[setCode] = current;
                }
                AddToSetStatistic(c, current);
            }
        }
        return result;
    }

    private static bool IsBasicLand(CardStats cs)
    {
        return cs.Info.Supertypes.Contains(1); // see Enums Table in CardDatabase
    }

    private static void AddToSetStatistic(CardStats cs, SetStatistic ss)
    {
        if (IsBasicLand(cs) || !cs.Collectible)
            return;

        int zeroOrOne = cs.Owned > 0 ? 1 : 0;
        ss.amountByRarity[cs.Info.Rarity] += cs.Owned;
        ss.amountByRarityUnique[cs.Info.Rarity] += zeroOrOne;
        ss.cardCountByRarity[cs.Info.Rarity] += 1;

        ss.TotalOwned += cs.Owned;
        ss.TotalOwnedUnique += zeroOrOne;
        ss.CardCount += 1;
    }
}

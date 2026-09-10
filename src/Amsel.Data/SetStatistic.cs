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
            AddToSetStatistic(c.Info.ExpansionCode, c, result);
            AddToSetStatistic(c.Info.DigitalReleaseSet, c, result);
        }
        return result;
    }

    private static bool IsBasicLand(CardStats cs)
    {
        return cs.Info.Supertypes.Contains(1); // see Enums Table in CardDatabase
    }

    private static void AddToSetStatistic(string code, CardStats cs,
        Dictionary<string, SetStatistic> result)
    {
        if (string.IsNullOrEmpty(code))
            return;
        if (!result.TryGetValue(code, out SetStatistic? current))
        {
            current = new SetStatistic();
            result.Add(code, current);
        }
        if (IsBasicLand(cs))
            return;

        int zeroOrOne = cs.Owned > 0 ? 1 : 0;
        current.amountByRarity[cs.Info.Rarity] += cs.Owned;
        current.amountByRarityUnique[cs.Info.Rarity] += zeroOrOne;
        current.cardCountByRarity[cs.Info.Rarity] += 1;

        current.TotalOwned += cs.Owned;
        current.TotalOwnedUnique += zeroOrOne;
        current.CardCount += 1;
    }
}

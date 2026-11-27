using System.Collections.Frozen;

namespace Amsel.Data.TextSearchFilter;

internal class QuantityOverallFilter(
    int amount,
    FilterOperator op,
    FrozenDictionary<uint, int> ownedByTitle) : ICardFilter
{
    private readonly int amount = amount;
    private readonly FilterOperator op = op;
    private readonly FrozenDictionary<uint, int> ownedByTitle = ownedByTitle;

    public bool Apply(CardStats c)
    {
        return op.Apply(ownedByTitle[c.Info.TitleId], amount);
    }

    public override bool Equals(object? other)
    {
        if (other is not QuantityOverallFilter rOther)
        {
            return false;
        }

        return amount == rOther.amount && op == rOther.op && ownedByTitle == rOther.ownedByTitle;
    }

    public override int GetHashCode()
    {
        return HashCode.Combine(amount, op, ownedByTitle);
    }
}

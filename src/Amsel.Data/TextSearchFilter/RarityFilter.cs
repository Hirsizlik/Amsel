namespace Amsel.Data.TextSearchFilter;

internal class RarityFilter(Rarity rarity, FilterOperator op) : ICardFilter
{
    private readonly Rarity rarity = rarity;
    private readonly FilterOperator op = op;

    public bool Apply(CardStats c)
    {
        return op.Apply((int)c.Info.Rarity, (int)rarity);
    }

    public override bool Equals(object? other)
    {
        if (other is not RarityFilter rOther)
        {
            return false;
        }

        return rarity == rOther.rarity && op == rOther.op;
    }

    public override int GetHashCode()
    {
        return HashCode.Combine(rarity, op);
    }
}

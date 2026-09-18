namespace Amsel.Data.TextSearchFilter;

internal class QuantityFilter(int amount, FilterOperator op) : ICardFilter
{
    private readonly int amount = amount;
    private readonly FilterOperator op = op;

    public bool Apply(CardStats c)
    {
        return op.Apply(c.Owned, amount);
    }

    public override bool Equals(object? other)
    {
        if (other is not QuantityFilter rOther)
        {
            return false;
        }

        return amount == rOther.amount && op == rOther.op;
    }

    public override int GetHashCode()
    {
        return HashCode.Combine(amount, op);
    }
}

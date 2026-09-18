namespace Amsel.Data.TextSearchFilter;

internal class FormatFilter(FormatInformation format, FilterOperator op) : ICardFilter
{
    private readonly FormatInformation format = format;
    private readonly FilterOperator op = op;

    public bool Apply(CardStats c)
    {
        bool banned = format.Data.BannedTitleIds.Contains(c.Info.TitleId)
            || format.Data.BannedAsCommanderTitles.Contains(c.Info.TitleId);
        bool legal = format.Data.LegalTitleIds.Contains(c.Info.TitleId);
        return op switch
        {
            FilterOperator.Equal => legal && !banned,
            FilterOperator.NotEqual => !legal || banned,
            _ => throw new InvalidOperationException("Unsupported Operation " + op)
        };
    }

    public override bool Equals(object? other)
    {
        if (other is not FormatFilter fOther)
        {
            return false;
        }

        return format == fOther.format && op == fOther.op;
    }

    public override int GetHashCode()
    {
        return HashCode.Combine(format, op);
    }
}

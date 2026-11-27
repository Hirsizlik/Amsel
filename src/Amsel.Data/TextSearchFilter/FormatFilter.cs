namespace Amsel.Data.TextSearchFilter;

internal enum FormatFilterType
{
    Any,
    Legal,
    Banned
}

internal class FormatFilter(FormatInformation format, FilterOperator op, FormatFilterType type) : ICardFilter
{
    private readonly FormatInformation format = format;
    private readonly FilterOperator op = op;
    private readonly FormatFilterType type = type;

    public bool Apply(CardStats c)
    {
        bool banned = format.Data.BannedTitleIds.Contains(c.Info.TitleId)
            || format.Data.BannedAsCommanderTitles.Contains(c.Info.TitleId)
            || format.Data.RestrictedTitleIds.ContainsKey(c.Info.TitleId);
        bool legal = format.Data.LegalTitleIds.Contains(c.Info.TitleId);
        
        return type switch
        {
            FormatFilterType.Any => op.Apply(legal || banned),
            FormatFilterType.Legal => op.Apply(legal),
            FormatFilterType.Banned => op.Apply(banned),
            _ => throw new ArgumentException("Invalid FormatFilterType")
        };
    }

    public override bool Equals(object? other)
    {
        if (other is not FormatFilter fOther)
        {
            return false;
        }

        return format == fOther.format && op == fOther.op && type == fOther.type;
    }

    public override int GetHashCode()
    {
        return HashCode.Combine(format, op, type);
    }
}

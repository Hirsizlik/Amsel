using System.Collections.Immutable;

namespace Amsel.Data.TextSearchFilter;

internal enum IsFilterType
{
    Collectible,
    Banned
}

internal class IsFilter(IsFilterType type, FilterOperator op, ImmutableArray<FormatInformation> formatInformations) : ICardFilter
{
    private readonly IsFilterType type = type;
    private readonly FilterOperator op = op;
    private readonly ImmutableArray<FormatInformation> formatInformations = formatInformations;

    private static bool IsBanned(CardStats s, FormatInformation f)
    {
        var id = s.Info.TitleId;
        return f.Data.BannedTitleIds.Contains(id) 
                || f.Data.BannedAsCommanderTitles.Contains(id)
                || f.Data.RestrictedTitleIds.ContainsKey(id);
    }

    public bool Apply(CardStats s)
    {
        return type switch
        {
            IsFilterType.Collectible => op.Apply(s.Collectible),
            IsFilterType.Banned => op.Apply(formatInformations.Any(f => IsBanned(s, f))),
            _ => throw new ArgumentException("Invalid IsFilterType")
        };
    }

    public override bool Equals(object? other)
    {
        if (other is not IsFilter fOther)
        {
            return false;
        }

        return type.Equals(fOther.type) 
            && op.Equals(fOther.op) 
            && formatInformations.Equals(fOther.formatInformations);
    }

    public override int GetHashCode()
    {
        return HashCode.Combine(type, op);
    }
}

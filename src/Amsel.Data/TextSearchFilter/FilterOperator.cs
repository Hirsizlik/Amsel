namespace Amsel.Data.TextSearchFilter;

internal enum FilterOperator
{
    Equal,
    NotEqual,
    Lower,
    LowerEqual,
    Higher,
    HigherEqual,
}

internal static class FilterOperatorExtension
{
    public static bool Apply(this FilterOperator op, int l, int r)
    {
        return op switch
        {
            FilterOperator.Equal => l == r,
            FilterOperator.NotEqual => l != r,
            FilterOperator.Higher => l > r,
            FilterOperator.HigherEqual => l >= r,
            FilterOperator.Lower => l < r,
            FilterOperator.LowerEqual => l <= r,
            _ => throw new ArgumentException("Unknown FilterOperator" + op)
        };
    }

    public static FilterOperator FromString(string s)
    {
        return s switch
        {
            "=" or ":" => FilterOperator.Equal,
            "!=" => FilterOperator.NotEqual,
            ">" => FilterOperator.Higher,
            ">=" => FilterOperator.HigherEqual,
            "<" => FilterOperator.Lower,
            "<=" => FilterOperator.LowerEqual,
            _ => throw new ArgumentException("Could not parse " + s)
        };
    }
}

namespace Amsel.Data.TextSearchFilter;

internal class NameFilter(string name) : ICardFilter
{
    private readonly string name = name.ToLowerInvariant();

    public bool Apply(CardStats s)
    {
        return s.Info.Name.Contains(name, StringComparison.OrdinalIgnoreCase);
    }

    public override bool Equals(object? other)
    {
        if (other is not NameFilter fOther)
        {
            return false;
        }

        return name.Equals(fOther.name);
    }

    public override int GetHashCode()
    {
        return name.GetHashCode();
    }
}

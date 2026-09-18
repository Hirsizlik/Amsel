namespace Amsel.Data.TextSearchFilter;

internal interface ICardFilter
{
    bool Apply(CardStats c);
    // also must implement Equals
}

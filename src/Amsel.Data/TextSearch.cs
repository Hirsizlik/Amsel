using System.ComponentModel;
using System.Text;
using System.Text.RegularExpressions;

namespace Amsel.Data;

internal interface ICardFilter
{
    bool Apply(CardStats c);
}

internal record NameFilter(string Name) : ICardFilter
{
    public bool Apply(CardStats s)
    {
        return s.Info.Name.Contains(Name, StringComparison.OrdinalIgnoreCase);
    }
}

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
            _ => throw new InvalidEnumArgumentException("Unknown FilterOperator" + op)
        };
    }

    public static FilterOperator FromString(string s)
    {
        return s switch
        {
            "=" => FilterOperator.Equal,
            "!=" => FilterOperator.NotEqual,
            ">" => FilterOperator.Higher,
            ">=" => FilterOperator.HigherEqual,
            "<" => FilterOperator.Lower,
            "<=" => FilterOperator.LowerEqual,
            _ => throw new InvalidEnumArgumentException("Could not parse " + s)
        };
    }
}

internal record RarityFilter(Rarity Rarity, FilterOperator Op) : ICardFilter
{
    public bool Apply(CardStats c)
    {
        return Op.Apply((int)c.Info.Rarity, (int)Rarity);
    }
}

internal record QuantityFilter(int Amount, FilterOperator Op) : ICardFilter
{
    public bool Apply(CardStats c)
    {
        return Op.Apply(c.Owned, Amount);
    }
}

public partial class TextSearch
{
    private readonly List<ICardFilter> filter;

    [GeneratedRegex("^R(<|<=|>|>=|=|!=)([LCURM])$", RegexOptions.IgnoreCase)]
    private static partial Regex RarityPattern { get; }
    [GeneratedRegex("^Q(<|<=|>|>=|=|!=)(\\d+)$", RegexOptions.IgnoreCase)]
    private static partial Regex QuantityPattern { get; }

    public static bool TryParse(string raw, out TextSearch result)
    {
        StringBuilder currentWord = new();
        bool inQuotes = false;
        List<string> words = [];
        List<ICardFilter> filter = [];
        void AddCurrentWord()
        {
            if (currentWord.Length > 0)
            {
                words.Add(currentWord.ToString());
                currentWord.Clear();
            }
        }
        foreach (char c in raw)
        {
            if (!inQuotes)
            {
                if (c == '"')
                {
                    inQuotes = true;
                    AddCurrentWord();
                }
                else if (c == ' ')
                {
                    AddCurrentWord();
                }
                else
                {
                    currentWord.Append(c);
                }
            }
            else if (inQuotes)
            {
                if (c == '"')
                {
                    inQuotes = false;
                    AddCurrentWord();
                }
                else
                {
                    currentWord.Append(c);
                }
            }
        }
        if (inQuotes)
        {
            // missing closing "
            result = new TextSearch(filter);
            return false;
        }
        AddCurrentWord(); // the last word if not already added

        foreach (string word in words)
        {
            if (RarityPattern.Match(word) is { Success: true } rarityMatch)
            {
                var op = FilterOperatorExtension.FromString(rarityMatch.Groups[1].Value);
                char rarityChar = rarityMatch.Groups[2].Value[0];
                filter.Add(new RarityFilter(
                    RarityExtension.FromChar(rarityChar),
                    op
                ));
                continue;
            }

            if (QuantityPattern.Match(word) is { Success: true } quantityMatch)
            {
                var op = FilterOperatorExtension.FromString(quantityMatch.Groups[1].Value);
                int quantity = int.Parse(quantityMatch.Groups[2].Value);
                filter.Add(new QuantityFilter(
                    quantity,
                    op
                ));
                continue;
            }

            // else just a name filter
            filter.Add(new NameFilter(word));
        }
        result = new TextSearch(filter);
        return true;
    }

    private TextSearch(List<ICardFilter> filter)
    {
        this.filter = filter;
    }

    public bool FilterCard(CardStats card)
    {
        bool result = true;
        foreach (var f in filter)
        {
            result &= f.Apply(card);
        }
        return result;
    }
}

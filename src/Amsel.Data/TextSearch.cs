using System.Collections.Frozen;
using System.ComponentModel;
using System.Text;
using System.Text.RegularExpressions;

namespace Amsel.Data;

internal interface ICardFilter
{
    bool Apply(CardStats c);
    // also must implement Equals
}

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
            "=" or ":" => FilterOperator.Equal,
            "!=" => FilterOperator.NotEqual,
            ">" => FilterOperator.Higher,
            ">=" => FilterOperator.HigherEqual,
            "<" => FilterOperator.Lower,
            "<=" => FilterOperator.LowerEqual,
            _ => throw new InvalidEnumArgumentException("Could not parse " + s)
        };
    }
}

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

public partial class TextSearch
{
    private readonly FrozenSet<ICardFilter> filter;

    [GeneratedRegex("^\\w+(<|<=|>|>=|=|:|!=)(.+)$")]
    private static partial Regex GenericPattern { get; }
    [GeneratedRegex("^R(<|<=|>|>=|=|:|!=)([LCURM])$", RegexOptions.IgnoreCase)]
    private static partial Regex RarityPattern { get; }
    [GeneratedRegex("^Q(<|<=|>|>=|=|:|!=)(\\d+)$", RegexOptions.IgnoreCase)]
    private static partial Regex QuantityPattern { get; }

    public static bool TryParse(string raw, out TextSearch result)
    {
        if (string.IsNullOrWhiteSpace(raw))
        {
            // empty search string is fine...
            result = new TextSearch([]);
            return true;
        }
        List<string> tokens = Tokenize(raw);
        if (tokens.Count == 0)
        {
            // ...but having no tokens after parsing is not
            result = new TextSearch([]);
            return false;
        }
        return TryParseOptions(tokens, out result);
    }

    private static bool TryParseOptions(List<string> tokens, out TextSearch textSearch)
    {
        List<ICardFilter> filter = [];
        foreach (string token in tokens)
        {
            if (GenericPattern.IsMatch(token))
            {
                if (RarityPattern.Match(token) is { Success: true } rarityMatch)
                {
                    var op = FilterOperatorExtension.FromString(rarityMatch.Groups[1].Value);
                    char rarityChar = rarityMatch.Groups[2].Value[0];
                    filter.Add(new RarityFilter(
                        RarityExtension.FromChar(rarityChar),
                        op
                    ));
                    continue;
                }

                if (QuantityPattern.Match(token) is { Success: true } quantityMatch)
                {
                    var op = FilterOperatorExtension.FromString(quantityMatch.Groups[1].Value);
                    int quantity = int.Parse(quantityMatch.Groups[2].Value);
                    filter.Add(new QuantityFilter(
                        quantity,
                        op
                    ));
                    continue;
                }
                textSearch = new TextSearch([]);
                return false;
            }
            else
            {
                filter.Add(new NameFilter(token));
            }
        }
        textSearch = new TextSearch(filter);
        return true;
    }

    private static List<string> Tokenize(string raw)
    {
        StringBuilder currentWord = new();
        bool inQuotes = false;
        List<string> words = [];
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
            return [];
        }
        AddCurrentWord(); // the last word if not already added
        return words;
    }

    private TextSearch(IEnumerable<ICardFilter> filter)
    {
        this.filter = filter.ToFrozenSet();
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

    public override bool Equals(object? obj)
    {
        if (obj is not TextSearch tObj)
        {
            return false;
        }
        return filter.SetEquals(tObj.filter);
    }

    public override int GetHashCode()
    {
        HashCode hc = new();
        foreach (var f in filter)
        {
            hc.Add(f);
        }
        return hc.ToHashCode();
    }
}

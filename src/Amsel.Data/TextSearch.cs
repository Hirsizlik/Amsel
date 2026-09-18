using System.Collections.Frozen;
using System.Collections.Immutable;
using System.Text;
using System.Text.RegularExpressions;
using Amsel.Data.TextSearchFilter;

namespace Amsel.Data;

public partial class TextSearch
{
    private readonly FrozenSet<ICardFilter> filter;

    [GeneratedRegex("^\\w+(<|<=|>|>=|=|:|!=)(.+)$")]
    private static partial Regex GenericPattern { get; }
    [GeneratedRegex("^R(<|<=|>|>=|=|:|!=)([LCURM])$", RegexOptions.IgnoreCase)]
    private static partial Regex RarityPattern { get; }
    [GeneratedRegex("^Q(<|<=|>|>=|=|:|!=)(\\d+)$", RegexOptions.IgnoreCase)]
    private static partial Regex QuantityPattern { get; }
    [GeneratedRegex("^F(=|:|!=)(\\w+)$", RegexOptions.IgnoreCase)]
    private static partial Regex FormatPattern { get; }

    public static bool TryParse(string raw, ImmutableArray<FormatInformation> formatInformation, out TextSearch result)
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
        return TryParseOptions(tokens, formatInformation, out result);
    }

    private static bool TryParseOptions(List<string> tokens, ImmutableArray<FormatInformation> formatInformation,
        out TextSearch textSearch)
    {
        List<ICardFilter> filter = [];
        foreach (string token in tokens)
        {
            if (!GenericPattern.IsMatch(token))
            {
                filter.Add(new NameFilter(token));
            }
            else if (RarityPattern.Match(token) is { Success: true } rarityMatch)
            {
                var op = FilterOperatorExtension.FromString(rarityMatch.Groups[1].Value);
                char rarityChar = rarityMatch.Groups[2].Value[0];
                filter.Add(new RarityFilter(
                    RarityExtension.FromChar(rarityChar),
                    op
                ));
            }
            else if (QuantityPattern.Match(token) is { Success: true } quantityMatch)
            {
                var op = FilterOperatorExtension.FromString(quantityMatch.Groups[1].Value);
                int quantity = int.Parse(quantityMatch.Groups[2].Value);
                filter.Add(new QuantityFilter(
                    quantity,
                    op
                ));
            }
            else if (FormatPattern.Match(token) is { Success: true } formatMatch)
            {
                string formatName = formatMatch.Groups[2].Value;
                FormatInformation? format = formatInformation
                    .FirstOrDefault(f => f.Name.Equals(formatName, StringComparison.OrdinalIgnoreCase));
                if (format == null)
                {
                    textSearch = new TextSearch([]);
                    return false;
                }
                var op = FilterOperatorExtension.FromString(formatMatch.Groups[1].Value);
                filter.Add(new FormatFilter(format, op));
            }
            else
            {
                textSearch = new TextSearch([]);
                return false;
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

    public static TextSearch Empty()
    {
        return new TextSearch([]);
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

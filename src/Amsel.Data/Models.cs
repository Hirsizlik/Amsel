namespace Amsel.Data;

public enum Rarity
{
    Unknown,
    Land,
    Common,
    Uncommon,
    Rare,
    MythicRare
}

public static class RarityExtension
{
    extension(Rarity r)
    {
        public char ToChar()
        {
            return r switch
            {
                Rarity.Land => 'L',
                Rarity.Common => 'C',
                Rarity.Uncommon => 'U',
                Rarity.Rare => 'R',
                Rarity.MythicRare => 'M',
                _ => '?'
            };
        }
    }

    public static Rarity FromChar(char c)
    {
        return c switch
        {
            'L' => Rarity.Land,
            'C' => Rarity.Common,
            'U' => Rarity.Uncommon,
            'R' => Rarity.Rare,
            'M' => Rarity.MythicRare,
            _ => Rarity.Uncommon
        };
    }
}

public record CardOwned(uint CardId, int Amount);

public record CardInfo(uint CardId, string Name, string ExpansionCode, string DigitalReleaseSet,
    uint CollectorNumber, uint? CollectorMax, Rarity Rarity, bool IsPrimary);

public record CardStats(CardInfo Info, int Owned);

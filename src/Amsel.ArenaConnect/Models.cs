namespace Amsel.ArenaConnect;

public enum Rarity
{
    Unknown,
    Land,
    Common,
    Uncommon,
    Rare,
    MythicRare
}

public record CardOwned(uint CardId, int Amount);

public record CardInfo(uint CardId, string Name, string ExpansionCode, string DigitalReleaseSet,
    uint CollectorNumber, uint? CollectorMax, Rarity Rarity, bool IsPrimary);

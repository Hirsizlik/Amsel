
using System.Collections.Frozen;

namespace Amsel.Data;

public readonly struct ExpansionSymbolKey(string code, Rarity rarity) : IEquatable<ExpansionSymbolKey>
{
    public string Code { get; } = SetRaritySymbolFixup.FixUpCode(code);
    public Rarity Rarity { get; } = SetRaritySymbolFixup.FixUpRarity(code, rarity);

    public static bool operator ==(ExpansionSymbolKey lhs, ExpansionSymbolKey rhs)
    {
        return lhs.Code == rhs.Code && lhs.Rarity == rhs.Rarity;
    }

    public static bool operator !=(ExpansionSymbolKey lhs, ExpansionSymbolKey rhs)
    {
        return !(lhs == rhs);
    }

    public bool Equals(ExpansionSymbolKey other)
    {
        return this == other;
    }

    public override bool Equals(object? obj)
    {
        return obj is ExpansionSymbolKey other && this == other;
    }

    public override int GetHashCode()
    {
        return HashCode.Combine(Code, Rarity);
    }
}

internal static class SetRaritySymbolFixup
{
    private static readonly FrozenSet<string> ReverseSets = FrozenSet.Create(["FACE", "MAR", "LIST", "BRAWL"]);
    // All non core sets before Exodus have only a "Common" Symbol (Core sets before 6ED have none)
    private static readonly FrozenSet<string> CommonOnlySets = FrozenSet.Create(["ARN", "ATQ", "LEG", "DRK", "FEM",
        "ICE", "HML", "ALL", "MIR", "VIS", "POR", "WTH", "TMP", "STH", ]);

    internal static string FixUpCode(string code)
    {
        if (code.Split('-') is [var first, var second, ..])
        {
            code = ReverseSets.Contains(first) ? second : first;
        }
        return code switch
        {
            // Secred lair and other Promos
            "SLD" or "SLC" or "APRM" or "EXTRALIFE2025" or "EUROLANDS" or "APACLANDS" or "PLANECATION" => "PRM",
            "G18" => "M19", // M19 gift pack
            "MAR" => "MSC", // MAR logo is not included (because red and usually not visible?)
            "AEFA" => "MH2", // MH2 Fetch Lands
            "YECL" => "LRW", // Lorwyn Eclipsed (use Lorwyn logo)
            "CONF" => "CON", // Conflux
            "SUMMER" => "INR", // "SUMMER-51.0", Land Promos from Innistrad Remastered
            // Alpha, Arena Beginner, Elspeth vs Ashiok, Cube cards, Momir
            "LEA" or "ANA" or "ANB" or "TGA19" or "CUBE" or "MOMIR" => "ARENA",
            // Arena Historic Anthologies
            string c when c.StartsWith("AHA") => "ARENA",
            // Arena, Explorer, Pioneer Anthologies
            string c when (c.StartsWith("AA") || c.StartsWith("EA") || c.StartsWith("PA"))
                && char.IsDigit(c[2]) => "ARENA",
            _ => code
        };
    }

    internal static Rarity FixUpRarity(string code, Rarity rarity)
    {
        // Lands use the common symbol
        if (rarity == Rarity.Land || CommonOnlySets.Contains(code))
        {
            return Rarity.Common;
        }
        return rarity;
    }
}

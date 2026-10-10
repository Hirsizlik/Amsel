
using System.Collections.Frozen;

namespace Amsel.Data;

public static class SetRaritySymbolFixup
{
    private static readonly FrozenSet<string> ReverseSets = FrozenSet.Create(["FACE", "MAR", "LIST", "BRAWL"]);

    public static string FixUpCode(string code)
    {
        if (code.Split('-') is [var first, var second, ..])
        {
            code = ReverseSets.Contains(first) ? second : first;
        }
        return code switch
        {
            // Secred lair and other Promos
            "SLD" or "APRM" or "EXTRALIFE2025" or "EUROLANDS" or "APACLANDS" or "PLANECATION" => "PRM",
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

    public static Rarity FixUpRarity(string code, Rarity rarity)
    {
        // Lands use the common symbol
        // Mirage has no separate symbols for Uncommon/Rare
        if (rarity == Rarity.Land || code == "MIR")
        {
            return Rarity.Common;
        }
        return rarity;
    }
}

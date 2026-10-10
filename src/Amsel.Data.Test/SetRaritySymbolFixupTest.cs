using SF = Amsel.Data.SetRaritySymbolFixup;

namespace Amsel.Data.Test;

public class SetRaritySymbolFixUpTest
{
    [Test]
    public void TestFixUpRarity()
    {
        var noChange = SF.FixUpRarity("WOE", Rarity.Rare);
        var mirToCommon = SF.FixUpRarity("MIR", Rarity.Rare);
        var landToCommon = SF.FixUpRarity("WOE", Rarity.Land);

        using (Assert.EnterMultipleScope())
        {
            Assert.That(noChange, Is.EqualTo(Rarity.Rare));
            Assert.That(mirToCommon, Is.EqualTo(Rarity.Common));
            Assert.That(landToCommon, Is.EqualTo(Rarity.Common));
        }
    }

    [Test]
    public void TestFixUpCode()
    {
        // not all variants, just one of each kind
        var simpleReplacement = SF.FixUpCode("SLD");
        var arenaHistoricAnthologies = SF.FixUpCode("AHA9");
        var otherAnthologies = SF.FixUpCode("EA9");
        var splitAndFixup = SF.FixUpCode("CUBE-52.60");
        var splitAndFixupTriple = SF.FixUpCode("ANA-GGJ-2026");
        var split = SF.FixUpCode("Y26-ECL");
        var reverseSplit = SF.FixUpCode("MAR-SPM");
        var unchanged = SF.FixUpCode("WOE");

        using (Assert.EnterMultipleScope())
        {
            Assert.That(simpleReplacement, Is.EqualTo("PRM"));
            Assert.That(arenaHistoricAnthologies, Is.EqualTo("ARENA"));
            Assert.That(otherAnthologies, Is.EqualTo("ARENA"));
            Assert.That(splitAndFixup, Is.EqualTo("ARENA"));
            Assert.That(splitAndFixupTriple, Is.EqualTo("ARENA"));
            Assert.That(split, Is.EqualTo("Y26"));
            Assert.That(reverseSplit, Is.EqualTo("SPM"));
            Assert.That(unchanged, Is.EqualTo("WOE"));
        }
    }
}

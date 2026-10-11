
namespace Amsel.Data.Test;

public class ExpansionSymbolKeyTest
{
    [Test]
    public void TestFixUpRarity()
    {
        var noChange = new ExpansionSymbolKey("WOE", Rarity.Rare);
        var mirToCommon = new ExpansionSymbolKey("MIR", Rarity.Rare);
        var landToCommon = new ExpansionSymbolKey("WOE", Rarity.Land);

        using (Assert.EnterMultipleScope())
        {
            Assert.That(noChange.Rarity, Is.EqualTo(Rarity.Rare));
            Assert.That(mirToCommon.Rarity, Is.EqualTo(Rarity.Common));
            Assert.That(landToCommon.Rarity, Is.EqualTo(Rarity.Common));
        }
    }

    [Test]
    public void TestFixUpCode()
    {
        // not all variants, just one of each kind
        var simpleReplacement = new ExpansionSymbolKey("SLD", Rarity.Common);
        var arenaHistoricAnthologies = new ExpansionSymbolKey("AHA9", Rarity.Common);
        var otherAnthologies = new ExpansionSymbolKey("EA9", Rarity.Common);
        var splitAndFixup = new ExpansionSymbolKey("CUBE-52.60", Rarity.Common);
        var splitAndFixupTriple = new ExpansionSymbolKey("ANA-GGJ-2026", Rarity.Common);
        var split = new ExpansionSymbolKey("Y26-ECL", Rarity.Common);
        var reverseSplit = new ExpansionSymbolKey("MAR-SPM", Rarity.Common);
        var unchanged = new ExpansionSymbolKey("WOE", Rarity.Common);

        using (Assert.EnterMultipleScope())
        {
            Assert.That(simpleReplacement.Code, Is.EqualTo("PRM"));
            Assert.That(arenaHistoricAnthologies.Code, Is.EqualTo("ARENA"));
            Assert.That(otherAnthologies.Code, Is.EqualTo("ARENA"));
            Assert.That(splitAndFixup.Code, Is.EqualTo("ARENA"));
            Assert.That(splitAndFixupTriple.Code, Is.EqualTo("ARENA"));
            Assert.That(split.Code, Is.EqualTo("Y26"));
            Assert.That(reverseSplit.Code, Is.EqualTo("SPM"));
            Assert.That(unchanged.Code, Is.EqualTo("WOE"));
        }
    }
}

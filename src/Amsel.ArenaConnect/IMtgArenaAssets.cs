using Amsel.Data;

namespace Amsel.ArenaConnect;

public interface IMtgArenaAssets
{
    byte[] GetExpansionSymbolPng(ExpansionSymbolKey key);

}

using Amsel.ArenaConnect;
using Amsel.Data;
using Avalonia;
using Avalonia.Media;
using Avalonia.Media.Imaging;
using Avalonia.Platform;
using CommunityToolkit.Mvvm.ComponentModel;
using System.IO;
using System.Linq;
using System.Threading.Tasks;

namespace Amsel.Avalonia.ViewModels;

public class SetInformationWithSymbol(SetInformation set, IMtgArenaAssets assets)
{
    public SetInformation Set { get; init; } = set;
    public IImage CommonSymbol
    {
        get
        {
            var key = new ExpansionSymbolKey(Set.Code, Rarity.Common);
            var png = assets.GetExpansionSymbolPng(key);
            return new Bitmap(new MemoryStream(png));
        }
    }
}

public partial class CardListViewModel : ViewModelBase
{
    private readonly ArenaLoader loader;
    private IMtgArenaAssets? arenaAssets;
    private readonly MtgArenaAssetsFactory arenaAssetsFactory;

    public SetInformationWithSymbol? SetWithSymbol { get; set; }

    public string Search { get; set; } = "";

    [ObservableProperty]
    public partial SetInformationWithSymbol[]? Sets { get; set; }

    public CardListViewModel(ArenaLoader loader, MtgArenaAssetsFactory arenaAssetsFactory)
    {
        this.loader = loader;
        this.arenaAssetsFactory = arenaAssetsFactory;
    }

    public Task LoadAsync()
    {
        arenaAssets = arenaAssetsFactory.Create();
        Sets = loader.SetInformation
            .Select(s => new SetInformationWithSymbol(s.Value, arenaAssets))
            .OrderBy(s => s.Set.Code)
            .ToArray();
        return Task.CompletedTask;
    }
}

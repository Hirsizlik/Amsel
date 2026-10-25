using Amsel.ArenaConnect;
using Amsel.Avalonia.Models;
using CommunityToolkit.Mvvm.ComponentModel;
using System.Linq;
using System.Threading.Tasks;

namespace Amsel.Avalonia.ViewModels;

public partial class CardListViewModel : ViewModelBase
{
    private readonly ArenaLoader loader;
    private IMtgArenaAssets? arenaAssets;
    private readonly MtgArenaAssetsFactory arenaAssetsFactory;

    public DecoratedSetInformation? SetWithSymbol { get; set; }

    public string Search { get; set; } = "";

    [ObservableProperty]
    public partial DecoratedSetInformation[]? Sets { get; set; }

    [ObservableProperty]
    public partial DecoratedCardStats[]? Cards { get; set; }

    public CardListViewModel(ArenaLoader loader, MtgArenaAssetsFactory arenaAssetsFactory)
    {
        this.loader = loader;
        this.arenaAssetsFactory = arenaAssetsFactory;
    }

    public Task LoadAsync()
    {
        arenaAssets = arenaAssetsFactory.Create();
        Sets = loader.SetInformation
            .Select(s => new DecoratedSetInformation(s.Value, arenaAssets))
            .OrderBy(s => s.Set.Code)
            .ToArray();
        Cards = loader.Cards
            .Select(c => new DecoratedCardStats(
                c,
                loader.SetInformation.First(s => s.Key == c.Info.ExpansionCode).Value,
                loader.SetInformation.FirstOrDefault(s => s.Key == c.Info.DigitalReleaseSet).Value,
                arenaAssets,
                loader.OwnedPerTitle,
                loader.FormatInfo))
            .ToArray();
        return Task.CompletedTask;
    }
}

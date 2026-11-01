using Amsel.ArenaConnect;
using Amsel.Avalonia.Models;
using Amsel.Data;
using CommunityToolkit.Mvvm.ComponentModel;
using System;
using System.Linq;
using System.Threading.Tasks;

namespace Amsel.Avalonia.ViewModels;

public partial class SetStatisticViewModel : ViewModelBase
{
    private readonly ArenaLoader loader;
    private readonly MtgArenaAssetsFactory arenaAssetsFactory;

    private IMtgArenaAssets? arenaAssets;

    [ObservableProperty]
    public partial DecoratedSetInformation[]? Sets { get; set; }

    public SetStatisticViewModel(ArenaLoader loader, MtgArenaAssetsFactory arenaAssetsFactory)
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

        return Task.CompletedTask;
    }
}

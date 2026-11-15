using System.Threading.Tasks;
using Amsel.ArenaConnect;
using CommunityToolkit.Mvvm.ComponentModel;

namespace Amsel.Avalonia.ViewModels;

public partial class MainViewModel(
    ArenaLoader loader,
    CardListViewModel cardListViewModel,
    SetStatisticViewModel setStatisticViewModel)
        : ViewModelBase
{
    private readonly ArenaLoader loader = loader;

    public CardListViewModel CardListViewModel { get; init; } = cardListViewModel;
    public SetStatisticViewModel SetStatisticViewModel { get; init; } = setStatisticViewModel;

    [ObservableProperty]
    public partial bool Ready { get; set; }= false;

    public async Task LoadAsync()
    {
        await loader.LoadCardInfoAsync();
        Ready = true;
    }
}

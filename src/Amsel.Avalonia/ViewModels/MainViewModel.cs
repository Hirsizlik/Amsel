using System.Threading.Tasks;
using Amsel.ArenaConnect;
using Amsel.Avalonia.Views;
using CommunityToolkit.Mvvm.ComponentModel;

namespace Amsel.Avalonia.ViewModels;

public class MainViewModel : ViewModelBase
{
    private readonly ArenaLoader loader;

    public CardListViewModel CardListViewModel { get; init; }

    public MainViewModel(ArenaLoader loader, CardListViewModel cardListViewModel)
    {
        this.loader = loader;
        CardListViewModel = cardListViewModel;
    }

    public async Task LoadAsync()
    {
        await loader.LoadCardInfoAsync();
    }

    public int ActiveTabIndex { get; set; } = 0;
}

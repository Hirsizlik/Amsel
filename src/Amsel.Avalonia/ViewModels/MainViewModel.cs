using CommunityToolkit.Mvvm.ComponentModel;

namespace Amsel.Avalonia.ViewModels;

public class MainViewModel : ViewModelBase
{
    public int ActiveTabIndex { get; set; } = 0;
}

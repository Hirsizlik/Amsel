using System;
using System.Linq;
using System.Threading.Tasks;
using Amsel.Avalonia.ViewModels;
using Amsel.Avalonia.Views.UserControls;
using Avalonia.Controls;
using Avalonia.Interactivity;

namespace Amsel.Avalonia.Views.Windows;

public partial class MainWindow : Window
{
    public MainWindow()
    {
        InitializeComponent();
    }

    protected override void OnLoaded(RoutedEventArgs e)
    {
        base.OnLoaded(e);
        var mainDataContext = DataContext as MainViewModel ?? throw new ArgumentNullException("No MainViewModel");
        var _ = mainDataContext.LoadAsync()
            .ContinueWith(t =>
            {
                mainDataContext.SetStatisticViewModel.LoadAsync();
                mainDataContext.CardListViewModel.LoadAsync();
            });
    }

    private void OnSetStatisticsDetailButtonClicked(object? sender, DetailsButtonClickedEventArgs e)
    {
        MainTabControl.SelectedIndex = 1;
        var mainDataContext = DataContext as MainViewModel ?? throw new ArgumentNullException("No MainViewModel");
        if (mainDataContext.CardListViewModel.Sets is { } sets)
            mainDataContext.CardListViewModel.SetWithSymbol = sets.First(s => s.Set.Code == e.SetCode);
    }
}

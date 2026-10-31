using System;
using Amsel.Avalonia.ViewModels;
using Avalonia.Controls;
using Avalonia.Input;
using Avalonia.Interactivity;

namespace Amsel.Avalonia.Views;

public partial class CardList : ContentPage
{
    public CardList()
    {
        InitializeComponent();
    }

    protected override void OnLoaded(RoutedEventArgs e)
    {
        base.OnLoaded(e);
        var cardListViewModel = DataContext as CardListViewModel 
            ?? throw new ArgumentNullException("No CardListViewModel");
        var _ = cardListViewModel.LoadAsync();
    }

    private void OnSearchKeyUp(object? source, KeyEventArgs e)
    {
        if (e.Key == Key.Enter)
        {
            e.Handled = true;
            (DataContext as CardListViewModel)?.OnSearch();
        } else if (e.Key == Key.Escape)
        {
            e.Handled = true;
            (DataContext as CardListViewModel)?.OnSearchClear();
        }
    }

    private void OnSearchHelpClicked(object? source, RoutedEventArgs e)
    {
        var help = new SearchHelpDialog();
        help.Show();
    }
}

using System;
using Amsel.Avalonia.ViewModels;
using Amsel.Avalonia.Views.Windows;
using Avalonia.Controls;
using Avalonia.Input;
using Avalonia.Interactivity;

namespace Amsel.Avalonia.Views.ContentPages;

public partial class CardList : ContentPage
{
    public CardList()
    {
        InitializeComponent();
    }

    private void OnSearchKeyUp(object? source, KeyEventArgs e)
    {
        if (e.Key == Key.Enter)
        {
            e.Handled = true;
            (DataContext as CardListViewModel)?.OnSearch();
        }
        else if (e.Key == Key.Escape)
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

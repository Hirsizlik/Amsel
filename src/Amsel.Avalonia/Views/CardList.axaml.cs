using System;
using Amsel.Avalonia.ViewModels;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Interactivity;
using Avalonia.Markup.Xaml;

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
        var cardListViewModel = DataContext as CardListViewModel ?? throw new ArgumentNullException("No CardListViewModel");
        var _ = cardListViewModel.LoadAsync();
    }
}

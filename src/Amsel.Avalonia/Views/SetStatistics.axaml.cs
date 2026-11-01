using System;
using Amsel.Avalonia.ViewModels;
using Avalonia.Controls;
using Avalonia.Interactivity;

namespace Amsel.Avalonia.Views;

public partial class SetStatistics : ContentPage
{
    public SetStatistics()
    {
        InitializeComponent();
    }

    protected override void OnLoaded(RoutedEventArgs e)
    {
        base.OnLoaded(e);
        var viewModel = DataContext as SetStatisticViewModel
            ?? throw new ArgumentNullException("No SetStatisticViewModel");
        var _ = viewModel.LoadAsync();
    }
}

using System;
using Amsel.Avalonia.Models;
using Amsel.Avalonia.ViewModels;
using Amsel.Avalonia.Views.UserControls;
using Avalonia.Controls;
using Avalonia.Controls.Primitives;
using Avalonia.Interactivity;

namespace Amsel.Avalonia.Views.ContentPages;

public partial class SetStatistics : ContentPage
{

    public static readonly RoutedEvent<DetailsButtonClickedEventArgs> DetailsButtonClickedEvent =
        RoutedEvent.Register<SetStatisticOptions, DetailsButtonClickedEventArgs>(
            nameof(DetailsButtonClicked),
            RoutingStrategies.Direct);

    public event EventHandler<DetailsButtonClickedEventArgs> DetailsButtonClicked
    {
        add => AddHandler(DetailsButtonClickedEvent, value);
        remove => RemoveHandler(DetailsButtonClickedEvent, value);
    }

    public SetStatistics()
    {
        InitializeComponent();
    }

    private void OnSetStatisticOptionsChanged(object? sender, RoutedEventArgs e)
    {
        var options = (OptionsChangedEventArgs) e;
        var viewModel = DataContext as SetStatisticViewModel;
        viewModel?.ReloadSetsFiltered(options);
    }

    private void OnSelectionChanged(object? sender, SelectionChangedEventArgs e)
    {
        var viewModel = DataContext as SetStatisticViewModel
            ?? throw new ArgumentNullException("No SetStatisticViewModel");
        var box = e.Source as SelectingItemsControl
            ?? throw new ArgumentException("Source not a SelectingItemsControl");
        var selectdItem = box.SelectedItem as DecoratedSetInformation;
        viewModel.SelectedSet = selectdItem;
    }

    private void OnDetailsButtonClicked(object? sender, DetailsButtonClickedEventArgs e)
    {
        RaiseEvent(new DetailsButtonClickedEventArgs(DetailsButtonClickedEvent, e.SetCode));
    }
}

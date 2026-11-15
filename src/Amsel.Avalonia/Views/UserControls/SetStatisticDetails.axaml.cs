
using System;
using Amsel.Avalonia.Models;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Interactivity;

namespace Amsel.Avalonia.Views.UserControls;

public class DetailsButtonClickedEventArgs(RoutedEvent? routedEvent, string setCode) : RoutedEventArgs(routedEvent)
{
    public string SetCode { get; } = setCode;
}

public partial class SetStatisticDetails : UserControl
{
    public static readonly StyledProperty<DecoratedSetInformation> SetInformationProperty =
        AvaloniaProperty.Register<SetStatisticDetails, DecoratedSetInformation>(nameof(SetInformation));

    public static readonly RoutedEvent<DetailsButtonClickedEventArgs> DetailsButtonClickedEvent =
        RoutedEvent.Register<SetStatisticOptions, DetailsButtonClickedEventArgs>(
            nameof(DetailsButtonClicked),
            RoutingStrategies.Direct);

    public DecoratedSetInformation SetInformation
    {
        get => GetValue(SetInformationProperty);
        set => SetValue(SetInformationProperty, value);
    }

    public event EventHandler<DetailsButtonClickedEventArgs> DetailsButtonClicked
    {
        add => AddHandler(DetailsButtonClickedEvent, value);
        remove => RemoveHandler(DetailsButtonClickedEvent, value);
    }

    public SetStatisticDetails()
    {
        InitializeComponent();
    }

    private void OnDetailsButtonClicked(object? sender, RoutedEventArgs e)
    {
        RaiseEvent(new DetailsButtonClickedEventArgs(DetailsButtonClickedEvent, SetInformation.Set.Code));
    }
}

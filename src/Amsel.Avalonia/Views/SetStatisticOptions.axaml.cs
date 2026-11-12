
using System;
using System.ComponentModel;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Interactivity;

namespace Amsel.Avalonia.Views;

public enum Sort
{
    Name,
    [Description("Set Code")]
    SetCode,
    [Description("Release Date")]
    ReleaseDate,
    [Description("1x Completion")]
    Completion1x,
    [Description("4x Completion")]
    Completion4x
}

public enum Direction
{
    [Description("↧")]
    Ascending,
    [Description("↥")]
    Descending
}

public class OptionsChangedEventArgs : RoutedEventArgs
{
    public Filter Filter { get; }
    public Sort Sort { get; }
    public Direction Direction { get; }

    public OptionsChangedEventArgs(RoutedEvent routedEvent, Filter Filter, Sort Sort, Direction Direction)
        : base(routedEvent)
    {
        this.Filter = Filter;
        this.Sort = Sort;
        this.Direction = Direction;
    }
}

public readonly record struct Filter(bool WithoutMetadata, bool WithoutName, bool WithoutProgress, bool WithSubsets);

public partial class SetStatisticOptions : UserControl
{
    public static readonly StyledProperty<Sort> SortProperty =
        AvaloniaProperty.Register<SetStatisticOptions, Sort>(nameof(Sort));
    public static readonly StyledProperty<Direction> DirectionProperty =
        AvaloniaProperty.Register<SetStatisticOptions, Direction>(nameof(Direction));

    public static readonly StyledProperty<bool> WithoutMetadataProperty =
        AvaloniaProperty.Register<SetStatisticOptions, bool>(nameof(WithoutMetadata));
    public static readonly StyledProperty<bool> WithoutNameProperty =
        AvaloniaProperty.Register<SetStatisticOptions, bool>(nameof(WithoutName));
    public static readonly StyledProperty<bool> WithoutProgressProperty =
        AvaloniaProperty.Register<SetStatisticOptions, bool>(nameof(WithoutProgress));
    public static readonly StyledProperty<bool> WithSubsetsProperty =
        AvaloniaProperty.Register<SetStatisticOptions, bool>(nameof(WithSubsets));

    public static readonly RoutedEvent<RoutedEventArgs> OptionsChangedEvent =
        RoutedEvent.Register<SetStatisticOptions, RoutedEventArgs>(nameof(OptionsChanged), RoutingStrategies.Direct);

    public Sort[] Sorts { get; } = Enum.GetValues<Sort>();

    public Sort Sort
    {
        get => GetValue(SortProperty);
        set => SetValue(SortProperty, value);
    }

    public Direction[] Directions { get; } = Enum.GetValues<Direction>();

    public Direction Direction
    {
        get => GetValue(DirectionProperty);
        set => SetValue(DirectionProperty, value);
    }

    public bool WithoutMetadata
    {
        get => GetValue(WithoutMetadataProperty);
        set => SetValue(WithoutMetadataProperty, value);
    }

    public bool WithoutName
    {
        get => GetValue(WithoutNameProperty);
        set => SetValue(WithoutNameProperty, value);
    }

    public bool WithoutProgress
    {
        get => GetValue(WithoutProgressProperty);
        set => SetValue(WithoutProgressProperty, value);
    }

    public bool WithSubsets
    {
        get => GetValue(WithSubsetsProperty);
        set => SetValue(WithSubsetsProperty, value);
    }

    public event EventHandler<RoutedEventArgs> OptionsChanged
    {
        add => AddHandler(OptionsChangedEvent, value);
        remove => RemoveHandler(OptionsChangedEvent, value);
    }

    public SetStatisticOptions()
    {
        InitializeComponent();
    }

    private void OnSortChanged(object? sender, SelectionChangedEventArgs e)
    {
        var args = new OptionsChangedEventArgs(OptionsChangedEvent,
            new Filter(WithoutMetadata, WithoutName, WithoutProgress, WithSubsets),
            (Sort)e.AddedItems[0]!,
            Direction);
        RaiseEvent(args);
    }

    private void OnDirectionChanged(object? sender, SelectionChangedEventArgs e)
    {
        var args = new OptionsChangedEventArgs(OptionsChangedEvent,
            new Filter(WithoutMetadata, WithoutName, WithoutProgress, WithSubsets),
            Sort,
            (Direction)e.AddedItems[0]!);
        RaiseEvent(args);
    }

    private void OnWithoutMetadataChanged(object? sender, RoutedEventArgs e)
    {
        var newChecked = (e.Source as CheckBox)?.IsChecked ?? false;
        var args = new OptionsChangedEventArgs(OptionsChangedEvent,
            new Filter(newChecked, WithoutName, WithoutProgress, WithSubsets),
            Sort,
            Direction);
        RaiseEvent(args);
    }

    private void OnWithoutNameChanged(object? sender, RoutedEventArgs e)
    {
        var newChecked = (e.Source as CheckBox)?.IsChecked ?? false;
        var args = new OptionsChangedEventArgs(OptionsChangedEvent,
                new Filter(WithoutMetadata, newChecked, WithoutProgress, WithSubsets),
                Sort,
                Direction);
        RaiseEvent(args);
    }
    private void OnWithoutProgressChanged(object? sender, RoutedEventArgs e)
    {
        var newChecked = (e.Source as CheckBox)?.IsChecked ?? false;
        var args = new OptionsChangedEventArgs(OptionsChangedEvent,
            new Filter(WithoutMetadata, WithoutName, newChecked, WithSubsets),
            Sort,
            Direction);
        RaiseEvent(args);
    }

    private void OnWithSubsetsChanged(object? sender, RoutedEventArgs e)
    {
        var newChecked = (e.Source as CheckBox)?.IsChecked ?? false;
        var args = new OptionsChangedEventArgs(OptionsChangedEvent,
            new Filter(WithoutMetadata, WithoutName, WithoutProgress, newChecked),
            Sort,
            Direction);
        RaiseEvent(args);
    }
}

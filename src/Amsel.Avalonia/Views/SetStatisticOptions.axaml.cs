
using System;
using System.ComponentModel;
using Avalonia;
using Avalonia.Controls;

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

    public SetStatisticOptions()
    {
        InitializeComponent();
    }
}

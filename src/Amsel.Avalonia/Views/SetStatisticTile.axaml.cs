using Amsel.Avalonia.Models;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Media;

namespace Amsel.Avalonia.Views;

public partial class SetStatisticTile : UserControl
{
    public static readonly StyledProperty<DecoratedSetInformation?> SetInformationProperty =
        AvaloniaProperty.Register<SetStatisticTile, DecoratedSetInformation?>(nameof(SetInformation));

    public DecoratedSetInformation? SetInformation
    {
        get => GetValue(SetInformationProperty);
        set => SetValue(SetInformationProperty, value);
    }

    public SetStatisticTile()
    {
        InitializeComponent();
    }
}

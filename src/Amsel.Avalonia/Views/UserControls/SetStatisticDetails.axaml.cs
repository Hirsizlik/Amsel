
using Amsel.Avalonia.Models;
using Avalonia;
using Avalonia.Controls;

namespace Amsel.Avalonia.Views.UserControls;

public partial class SetStatisticDetails : UserControl
{
    public static readonly StyledProperty<DecoratedSetInformation> SetInformationProperty =
        AvaloniaProperty.Register<SetStatisticDetails, DecoratedSetInformation>(nameof(SetInformation));

    public DecoratedSetInformation SetInformation
    {
        get => GetValue(SetInformationProperty);
        set => SetValue(SetInformationProperty, value);
    }

    public SetStatisticDetails()
    {
        InitializeComponent();
    }

}

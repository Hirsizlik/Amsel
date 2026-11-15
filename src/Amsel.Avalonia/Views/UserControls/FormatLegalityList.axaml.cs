using Amsel.Avalonia.Models;
using Avalonia;
using Avalonia.Controls;

namespace Amsel.Avalonia.Views.UserControls;

public partial class FormatLegalityList : UserControl
{
    public static readonly StyledProperty<FormatLegality[]> FormatLegalitiesProperty =
        AvaloniaProperty.Register<SetEntryElement, FormatLegality[]>(nameof(FormatLegalities));

    public FormatLegality[] FormatLegalities
    {
        get => GetValue(FormatLegalitiesProperty);
        set => SetValue(FormatLegalitiesProperty, value);
    }

    public FormatLegalityList()
    {
        InitializeComponent();
    }

}

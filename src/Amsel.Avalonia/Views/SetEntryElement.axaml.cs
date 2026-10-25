using Avalonia;
using Avalonia.Controls;
using Avalonia.Media;

namespace Amsel.Avalonia.Views;

public partial class SetEntryElement : UserControl
{
    public SetEntryElement()
    {
        InitializeComponent();
    }

    public static readonly StyledProperty<string?> SetNameProperty =
        AvaloniaProperty.Register<SetEntryElement, string?>(nameof(SetName));
    public static readonly StyledProperty<string?> CodeProperty =
        AvaloniaProperty.Register<SetEntryElement, string?>(nameof(Code));
    public static readonly StyledProperty<IImage?> SymbolProperty =
        AvaloniaProperty.Register<SetEntryElement, IImage?>(nameof(Symbol));

    public string? SetName
    {
        get => GetValue(SetNameProperty);
        set => SetValue(SetNameProperty, value);
    }

    public string? Code
    {
        get => GetValue(CodeProperty);
        set => SetValue(CodeProperty, value);
    }

    public IImage? Symbol
    {
        get => GetValue(SymbolProperty);
        set => SetValue(SymbolProperty, value);
    }
}

using System;
using Amsel.Avalonia.ViewModels;
using Avalonia.Controls;
using Avalonia.Interactivity;

namespace Amsel.Avalonia.Views;

public partial class MainWindow : Window
{
    public MainWindow()
    {
        InitializeComponent();
    }

    protected override void OnLoaded(RoutedEventArgs e)
    {
        base.OnLoaded(e);
        var mainDataContext = DataContext as MainViewModel ?? throw new ArgumentNullException("No MainViewModel");
        var _ = mainDataContext.LoadAsync();
    }
}

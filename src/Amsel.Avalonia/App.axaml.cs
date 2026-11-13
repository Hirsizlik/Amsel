using Avalonia;
using Avalonia.Controls.ApplicationLifetimes;
using Avalonia.Markup.Xaml;
using Amsel.Avalonia.ViewModels;
using Amsel.ArenaConnect;
using Microsoft.Extensions.DependencyInjection;
using Xdg.Directories;
using Amsel.Avalonia.Views.Windows;

namespace Amsel.Avalonia;

public partial class App : Application
{
    public override void Initialize()
    {
        AvaloniaXamlLoader.Load(this);
    }

    public override void OnFrameworkInitializationCompleted()
    {
        var serviceCollection = new ServiceCollection();
        serviceCollection
            .AddSingleton<ArenaLoader>()
            .AddSingleton(new AmselSettings(
                BaseDirectory.CacheFile("Amsel/")
            ))
            .AddSingleton<CardListViewModel>()
            .AddSingleton<SetStatisticViewModel>()
            .AddSingleton<MainViewModel>()
            .AddSingleton<MtgArenaAssetsFactory>();
        
        var services = serviceCollection.BuildServiceProvider();
        if (ApplicationLifetime is IClassicDesktopStyleApplicationLifetime desktop)
        {
            desktop.MainWindow = new MainWindow
            {
                DataContext = services.GetRequiredService<MainViewModel>()
            };
        }

        base.OnFrameworkInitializationCompleted();
    }
}

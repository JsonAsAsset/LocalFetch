using Avalonia.Controls.ApplicationLifetimes;
using Avalonia.Markup.Xaml;
using LocalFetch.Services;
using LocalFetch.Windows;

namespace LocalFetch.Application;

public partial class App : Avalonia.Application
{
    public override void Initialize()
    {
        AvaloniaXamlLoader.Load(this);
    }
    
    public override void OnFrameworkInitializationCompleted()
    {
        if (ApplicationLifetime is IClassicDesktopStyleApplicationLifetime desktop)
        {
            ApplicationService.Application = desktop;
            ApplicationService.Initialize();
            
            desktop.MainWindow = new AppWindow();
        }

        base.OnFrameworkInitializationCompleted();
    }
}
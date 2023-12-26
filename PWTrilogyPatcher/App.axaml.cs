using Avalonia;
using Avalonia.Controls.ApplicationLifetimes;
using Avalonia.Data.Core.Plugins;
using Avalonia.Markup.Xaml;
using Microsoft.Extensions.DependencyInjection;
using PWTrilogyPatcher.Services;
using PWTrilogyPatcher.ViewModels;
using PWTrilogyPatcher.Views;
using System;

namespace PWTrilogyPatcher;

public partial class App : Application
{
    public new static App? Current => Application.Current as App;
    public static bool UpdateMode { get; private set; } = false;

    /// <summary>
    /// Gets the <see cref="IServiceProvider"/> instance to resolve application services.
    /// </summary>
    public IServiceProvider? Services { get; private set; }

    public override void Initialize()
    {
        AvaloniaXamlLoader.Load(this);
    }

    public override void OnFrameworkInitializationCompleted()
    {
        // Line below is needed to remove Avalonia data validation.
        // Without this line you will get duplicate validations from both Avalonia and CT
        BindingPlugins.DataValidators.RemoveAt(0);

        var services = new ServiceCollection();

        if (ApplicationLifetime is IClassicDesktopStyleApplicationLifetime desktop)
        {
            // Legacy Steam Update
            var args = desktop.Args;

            for (int i = 0; i < args.Length; i++)
            {
                if (args[i] == "-u")
                {
                    UpdateMode = true;
                }
            }

            desktop.MainWindow = new MainWindow
            {
                DataContext = new MainWindowViewModel()
            };

            services.AddSingleton<IStorageService>(x => new StorageService(desktop.MainWindow));
            
        }
        else if (ApplicationLifetime is ISingleViewApplicationLifetime singleViewPlatform)
        {
            singleViewPlatform.MainView = new MainView
            {
                DataContext = new MainWindowViewModel()
            };
        }

        Services = services.BuildServiceProvider();
        base.OnFrameworkInitializationCompleted();
    }

    public void Shutdown()
    {
        if (App.Current?.ApplicationLifetime is IClassicDesktopStyleApplicationLifetime desktop)
        {
            desktop.Shutdown();
        }
        else if (App.Current?.ApplicationLifetime is ISingleViewApplicationLifetime singleViewPlatform)
        {
            singleViewPlatform.MainView = null;
        }
    }
}

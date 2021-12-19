using AATrilogyPatcherSteam.ViewModels;
using AATrilogyPatcherSteam.Views;
using Avalonia;
using Avalonia.Controls.ApplicationLifetimes;
using Avalonia.Markup.Xaml;

namespace AATrilogyPatcherSteam
{
    public class App : Application
    {
        public override void Initialize()
        {
            AvaloniaXamlLoader.Load(this);
        }

        public override void OnFrameworkInitializationCompleted()
        {
            if (ApplicationLifetime is IClassicDesktopStyleApplicationLifetime desktop)
            {
                bool isUpdate = false;

                for (int i = 0; i < Program.launchArgs.Length; i++)
                {
                    if (Program.launchArgs[i] == "-u")
                        isUpdate = true;
                }

                var mainWindow = new MainWindow();
                mainWindow.UpdateMode(isUpdate);

                var mainWindowViewModel = new MainWindowViewModel();
                if (isUpdate)
                {
                    mainWindowViewModel.StartUpdatePatch();
                }

                mainWindow.DataContext = mainWindowViewModel;

                desktop.MainWindow = mainWindow;
            }

            base.OnFrameworkInitializationCompleted();
        }
    }
}

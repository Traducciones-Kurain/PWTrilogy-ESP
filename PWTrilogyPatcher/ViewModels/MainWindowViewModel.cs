using CommunityToolkit.Mvvm.ComponentModel;

namespace PWTrilogyPatcher.ViewModels;

public partial class MainWindowViewModel : ViewModelBase
{
    [ObservableProperty]
    private ViewModelBase contentViewModel;

    public MainWindowViewModel()
    {
        if (App.UpdateMode)
        {
            SetPatcherViewModel();
        }
        else
        {
            SetMainViewModel();
        }
    }

    public void SetMainViewModel()
    {
        ContentViewModel = new MainViewModel();
    }

    public void SetPatcherViewModel()
    {
        ContentViewModel = new PatcherViewModel();
    }
}

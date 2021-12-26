using Avalonia;
using Avalonia.Controls;
using Avalonia.Controls.ApplicationLifetimes;
using Avalonia.Interactivity;
using Avalonia.Markup.Xaml;
using Avalonia.Input;
using System.Diagnostics;

namespace AATrilogyPatcher.Views
{
    public partial class PatchWindow : UserControl
    {
        private bool isUpdate = false;
        public PatchWindow()
        {
            InitializeComponent();
            Initialized += PatchWindow_Initialized;
        }

        public void UpdateMode(bool _isUpdate)
        {
            if (_isUpdate)
                isUpdate = _isUpdate;
        }

        private void InitializeComponent()
        {
            AvaloniaXamlLoader.Load(this);
        }

        private void QuitPatcher()
        {
            if (Application.Current.ApplicationLifetime is IClassicDesktopStyleApplicationLifetime lifetime)
            {
                if (Sound.soundCtrl.audioPlayback != null)
                {
                    Sound.soundCtrl.audioPlayback.Dispose();
                }

                lifetime.Shutdown();
            }
        }

        private void PatchWindow_Initialized(object sender, System.EventArgs e)
        {
            if (DataContext is ViewModels.ICloseWindows vm)
            {
                vm.Close += () =>
                {
                    QuitPatcher();
                };
            }
        }

        private void SiClick(object sender, RoutedEventArgs e)
        {
            Sound.soundCtrl.PlaySound(AATrilogyPatcher.Resources.se001);
        }

        private void NoClick(object sender, RoutedEventArgs e)
        {
            Sound.soundCtrl.PlaySound(AATrilogyPatcher.Resources.se002);
            IsVisible = false;
        }

        private void AceptarClick(object sender, RoutedEventArgs e)
        {
            Sound.soundCtrl.PlaySound(AATrilogyPatcher.Resources.se001);
            if (!isUpdate)
                this.IsVisible = false;
            else
                QuitPatcher();
        }

        private void OnPointerEnter(object sender, PointerEventArgs e)
        {
            Sound.soundCtrl.PlaySound(AATrilogyPatcher.Resources.se000);
        }

        private void VerificarClick(object sender, RoutedEventArgs e)
        {
            Sound.soundCtrl.PlaySound(AATrilogyPatcher.Resources.se001);
            Process.Start(new ProcessStartInfo("cmd", "/c start steam://validate/787480") { CreateNoWindow = true });
        }
    }
}

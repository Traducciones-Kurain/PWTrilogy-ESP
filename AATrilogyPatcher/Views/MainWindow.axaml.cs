using Avalonia;
using Avalonia.Controls;
using Avalonia.Controls.ApplicationLifetimes;
using Avalonia.Interactivity;
using Avalonia.Markup.Xaml;
using Avalonia.Input;
using System.Diagnostics;
using System.Net;
using System.Runtime.InteropServices;

namespace AATrilogyPatcher.Views
{
    public partial class MainWindow : Window
    {
        private readonly Grid mainGrid;
        public MainWindow()
        {
            InitializeComponent();
            mainGrid = this.FindControl<Grid>("MainGrid");
#if DEBUG
            this.AttachDevTools();
#endif
        }

        public void UpdateMode(bool isUpdate)
        {
            if (isUpdate)
            {
                var patchWindow = new PatchWindow();
                patchWindow.UpdateMode(true);
                mainGrid.Children.Add(patchWindow);
            }
        }

        private void InitializeComponent()
        {
            AvaloniaXamlLoader.Load(this);
        }

        private void ParchearClick(object sender, RoutedEventArgs e)
        {
#if RELEASE_WINDOWS
            Sound.soundCtrl.PlaySound(AATrilogyPatcher.Resources.se001);
#endif
            mainGrid.Children.Add(new PatchWindow());
        }

        private void CreditosClick(object sender, RoutedEventArgs e)
        {
#if RELEASE_WINDOWS
            Sound.soundCtrl.PlaySound(AATrilogyPatcher.Resources.se001);
#endif
            OpenBrowser("https://github.com/Traducciones-Kurain/AATrilogy-2019-ESP/blob/master/README.md#cr%C3%A9ditos");
        }

        private void DiscordClick(object sender, RoutedEventArgs e)
        {
#if RELEASE_WINDOWS
            Sound.soundCtrl.PlaySound(AATrilogyPatcher.Resources.se001);
#endif

            // lo hago de esta manera por si el invite cambia y el usuario no esta forzado a actualizar el parcheador
            try
            {
                using (WebClient client = new WebClient())
                {
                    string rawInviteUrl = "https://raw.githubusercontent.com/Traducciones-Kurain/AATrilogy-2019-ESP/master/invite.txt";
                    string discordInvite = client.DownloadString(rawInviteUrl);

                    OpenBrowser(discordInvite);
                }
            }
            catch {}
        }

        private void SalirClick(object sender, RoutedEventArgs e)
        {
#if RELEASE_WINDOWS
            Sound.soundCtrl.PlaySound(AATrilogyPatcher.Resources.se001);
#endif

            if (Application.Current.ApplicationLifetime is IClassicDesktopStyleApplicationLifetime lifetime)
            {
#if RELEASE_WINDOWS
                if (Sound.soundCtrl.audioPlayback != null)
                {
                    Sound.soundCtrl.audioPlayback.Dispose();
                }
#endif

                lifetime.Shutdown();
            }
        }

        private void OnPointerEnter(object sender, PointerEventArgs e)
        {
#if RELEASE_WINDOWS
            Sound.soundCtrl.PlaySound(AATrilogyPatcher.Resources.se000);
#endif
        }

        private void TkClick(object sender, RoutedEventArgs e)
        {
            OpenBrowser("https://twitter.com/Trad_Kurain");
        }

        private void TsClick(object sender, RoutedEventArgs e)
        {
            OpenBrowser("https://tradusquare.es/");
        }

        //https://brockallen.com/2016/09/24/process-start-for-urls-on-net-core/
        public static void OpenBrowser(string url)
        {
            try
            {
                Process.Start(url);
            }
            catch
            {
                // hack because of this: https://github.com/dotnet/corefx/issues/10361
                if (RuntimeInformation.IsOSPlatform(OSPlatform.Windows))
                {
                    url = url.Replace("&", "^&");
                    Process.Start(new ProcessStartInfo("cmd", $"/c start {url}") { CreateNoWindow = true });
                }
                else if (RuntimeInformation.IsOSPlatform(OSPlatform.Linux))
                {
                    Process.Start("xdg-open", url);
                }
                else if (RuntimeInformation.IsOSPlatform(OSPlatform.OSX))
                {
                    Process.Start("open", url);
                }
                else
                {
                    throw;
                }
            }
        }
    }
}

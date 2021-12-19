using Avalonia;
using Avalonia.Controls;
using Avalonia.Controls.ApplicationLifetimes;
using Avalonia.Interactivity;
using Avalonia.Markup.Xaml;
using Avalonia.Input;
using System.Diagnostics;
using System.Net;

namespace AATrilogyPatcherSteam.Views
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

        private void InitializeComponent()
        {
            AvaloniaXamlLoader.Load(this);
        }

        private void ParchearClick(object sender, RoutedEventArgs e)
        {
            Sound.soundCtrl.PlaySound(AATrilogyPatcherSteam.Resources.se001);
            mainGrid.Children.Add(new PatchWindow());
        }

        private void CreditosClick(object sender, RoutedEventArgs e)
        {
            Sound.soundCtrl.PlaySound(AATrilogyPatcherSteam.Resources.se001);
        }

        private void DiscordClick(object sender, RoutedEventArgs e)
        {
            Sound.soundCtrl.PlaySound(AATrilogyPatcherSteam.Resources.se001);

            // lo hago de esta manera por si el invite cambia y el usuario no esta forzado a actualizar el parcheador
            try
            {
                using (WebClient client = new WebClient())
                {
                    // test url
                    string rawInviteUrl = "https://raw.githubusercontent.com/WorstAquaPlayer/test123/main/texto%20test.txt";
                    string discordInvite = client.DownloadString(rawInviteUrl);

                    Debug.WriteLine(discordInvite);
                    Process.Start("explorer", discordInvite);
                }
            }
            catch {}
        }

        private void SalirClick(object sender, RoutedEventArgs e)
        {
            Sound.soundCtrl.PlaySound(AATrilogyPatcherSteam.Resources.se001);

            if (Application.Current.ApplicationLifetime is IClassicDesktopStyleApplicationLifetime lifetime)
            {
                Sound.soundCtrl.audioPlayback.Dispose();

                lifetime.Shutdown();
            }
        }

        private void OnPointerEnter(object sender, PointerEventArgs e)
        {
            Sound.soundCtrl.PlaySound(AATrilogyPatcherSteam.Resources.se000);
        }
    }
}

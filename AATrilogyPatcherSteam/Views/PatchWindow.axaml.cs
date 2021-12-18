using Avalonia.Controls;
using Avalonia.Interactivity;
using Avalonia.Markup.Xaml;
using Avalonia.Input;

namespace AATrilogyPatcherSteam.Views
{
    public partial class PatchWindow : UserControl
    {
        public PatchWindow()
        {
            InitializeComponent();
        }

        private void InitializeComponent()
        {
            AvaloniaXamlLoader.Load(this);
        }

        private void SiClick(object sender, RoutedEventArgs e)
        {
            Sound.soundCtrl.PlaySound(AATrilogyPatcherSteam.Resources.se001);
        }

        private void NoClick(object sender, RoutedEventArgs e)
        {
            Sound.soundCtrl.PlaySound(AATrilogyPatcherSteam.Resources.se002);
            IsVisible = false;
        }

        private void AceptarClick(object sender, RoutedEventArgs e)
        {
            Sound.soundCtrl.PlaySound(AATrilogyPatcherSteam.Resources.se001);
            this.IsVisible = false;
        }

        private void OnPointerEnter(object sender, PointerEventArgs e)
        {
            Sound.soundCtrl.PlaySound(AATrilogyPatcherSteam.Resources.se000);
        }
    }
}

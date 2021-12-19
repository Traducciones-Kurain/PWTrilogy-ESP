using System.IO;
using System.Reactive;
using System.Threading.Tasks;
using Microsoft.Win32;
using ReactiveUI;

namespace AATrilogyPatcherSteam.ViewModels
{
    public class MainWindowViewModel : ViewModelBase
    {
        public static string steamGamePath { get; set; }

        private string _steamPath;
        public string steamPath
        {
            get => _steamPath;
            set => this.RaiseAndSetIfChanged(ref _steamPath, value);
        }

        private string _PlaySource;
        public string PlaySource
        {
            get => _PlaySource;
            set => this.RaiseAndSetIfChanged(ref _PlaySource, value);
        }

        private bool _errorVisible;
        public bool errorVisible
        {
            get => _errorVisible;
            set => this.RaiseAndSetIfChanged(ref _errorVisible, value);
        }

        private bool _aceptarVisible;
        public bool aceptarVisible
        {
            get => _aceptarVisible;
            set => this.RaiseAndSetIfChanged(ref _aceptarVisible, value);
        }

        private bool _patchVisible;
        public bool patchVisible
        {
            get => _patchVisible;
            set => this.RaiseAndSetIfChanged(ref _patchVisible, value);
        }

        private bool _patchingVisible;
        public bool patchingVisible
        {
            get => _patchingVisible;
            set => this.RaiseAndSetIfChanged(ref _patchingVisible, value);
        }

        public ReactiveCommand<Unit, Unit> findPath { get; }
        public ReactiveCommand<Unit, Unit> startPatch { get; }
        public ReactiveCommand<Unit, Unit> restartButtons { get; }

        private string assetsImgPath = "avares://AATrilogyPatcherSteam/Assets/img";

        public MainWindowViewModel()
        {
            errorVisible = false;
            patchVisible = false;
            aceptarVisible = false;
            PlaySource = $"{assetsImgPath}/ventana.png";
            steamPath = "";

            findPath = ReactiveCommand.Create(FindPath);
            startPatch = ReactiveCommand.CreateFromTask(StartPatch);
            restartButtons = ReactiveCommand.Create(RestartButtons);
        }

        void FindPath()
        {
            if (SteamKeyExists())
            {
                steamPath = steamGamePath;
                PlaySource = $"{assetsImgPath}/ventana_ok.png";
                patchVisible = true;
            }
            else
            {
                PlaySource = $"{assetsImgPath}/ventana_error_steam.png";
                aceptarVisible = true;
                errorVisible = true;
            }
        }

        private bool SteamKeyExists()
        {
            var localMachine = RegistryKey.OpenBaseKey(RegistryHive.LocalMachine, RegistryView.Registry64);
            var steamRegistry = localMachine.OpenSubKey(@"SOFTWARE\Microsoft\Windows\CurrentVersion\Uninstall\Steam App 787480");

            if (steamRegistry != null)
            {
                steamGamePath = (string)steamRegistry.GetValue("InstallLocation");
                return true;
            }

            return false;
        }

        async Task StartPatch()
        {
            await ApplyingPatch();
            await PatchingProcess();
        }

        async Task ApplyingPatch()
        {
            await Task.Run(() =>
            {
                patchVisible = false;
                patchingVisible = true;
                PlaySource = $"{assetsImgPath}/ventana_apl.png";
            });
        }

        async Task PatchingProcess()
        {
            await Task.Run(() =>
            {
                var patchResult = Patch.Patch.PatchProcess();
                if (patchResult.Item1 > 0)
                {
                    FailedPatch(patchResult.Item1, patchResult.Item2);
                }
                else
                {
                    patchingVisible = false;
                    aceptarVisible = true;
                    PlaySource = $"{assetsImgPath}/ventana_exito.png";
                }
            });
        }

        private void RestartButtons()
        {
            errorVisible = false;
            patchVisible = false;
            patchingVisible = false;
            aceptarVisible = false;
        }

        private void RestartWindow()
        {
            errorVisible = false;
            patchVisible = false;
            patchingVisible = false;
            PlaySource = $"{assetsImgPath}/ventana.png";
        }

        public void FailedPatch(int ErrorCode, string ErrorMessage)
        {
            RestartWindow();

            if (ErrorMessage != string.Empty)
            {
                switch (ErrorCode)
                {
                    case (int)ErrorCodes.DownloadError:
                        PlaySource = $"{assetsImgPath}/ventana_error_descarga.png";
                        break;
                    case (int)ErrorCodes.ExtractError:
                        PlaySource = $"{assetsImgPath}/ventana_error_extraer.png";
                        break;
                    case (int)ErrorCodes.PatchError:
                        PlaySource = $"{assetsImgPath}/ventana_error_aplicar.png";
                        break;
                }

                aceptarVisible = true;
                var errorLogFile = $"{Directory.GetCurrentDirectory()}{Path.DirectorySeparatorChar}ErrorLog.txt";
                File.WriteAllText(errorLogFile, ErrorMessage);
            }
        }

        public enum ErrorCodes : int
        {
            DownloadError = 1,
            ExtractError = 2,
            PatchError = 3
        }
    }
}

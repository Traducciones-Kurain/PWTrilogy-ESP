using Avalonia.Threading;
using System;
using System.Diagnostics;
using System.IO;
using System.Reactive;
using System.Threading.Tasks;
using Microsoft.Win32;
using ReactiveUI;

namespace AATrilogyPatcher.ViewModels
{
    public class MainWindowViewModel : ViewModelBase, ICloseWindows
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

        private bool _cacheVisible;
        public bool cacheVisible
        {
            get => _cacheVisible;
            set => this.RaiseAndSetIfChanged(ref _cacheVisible, value);
        }

        private string _textTest = "";
        public string textTest
        {
            get => _textTest;
            set => this.RaiseAndSetIfChanged(ref _textTest, value);
        }

        private bool updateMode;

        public ReactiveCommand<Unit, Unit> findPath { get; }
        public ReactiveCommand<Unit, Unit> startPatch { get; }
        public ReactiveCommand<Unit, Unit> restartButtons { get; }

        private string assetsImgPath = "avares://AATrilogyPatcher/Assets/img";

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
            updateMode = false;
            await StartPatchingProcess(updateMode);
        }

        public async Task StartUpdatePatch()
        {
            updateMode = true;

            if (SteamKeyExists())
            {
                steamPath = steamGamePath;
                await StartPatchingProcess(updateMode);
            }
        }

        async Task<(int, string)> PatchingProcess(bool isUpdate)
        {
            patchVisible = false;
            patchingVisible = true;
            PlaySource = $"{assetsImgPath}/ventana_apl.png";

            string downloadPath = $"{Directory.GetCurrentDirectory()}{Path.DirectorySeparatorChar}Patch-Steam.zip";
            var patchProcess = new Patch.PatchAsync(steamPath, downloadPath);

            textTest = "descargandoooo!";

            if (!File.Exists(downloadPath))
            {
                var downloadPatch = await patchProcess.DownloadPatch();
                if (!downloadPatch.Item1)
                {
                    return ((int)MainWindowViewModel.ErrorCodes.DownloadError, downloadPatch.Item2);
                }
            }

            textTest = "extrayendoooo!";

            var extractPatch = await patchProcess.ExtractPatch();
            if (!extractPatch.Item1)
            {
                return ((int)MainWindowViewModel.ErrorCodes.ExtractError, extractPatch.Item2);
            }

            textTest = "aplicandoooo!";

            var patchGame = await patchProcess.PatchGame();
            if (patchGame.Item1 > 0 && patchGame.Item1 != 1)
            {
                return ((int)MainWindowViewModel.ErrorCodes.PatchError, patchGame.Item2);
            }
            else if (patchGame.Item1 == 1)
            {
                return ((int)MainWindowViewModel.ErrorCodes.HashError, patchGame.Item2);
            }

            File.Delete(downloadPath);

            return (0, string.Empty);
        }

        async Task StartPatchingProcess(bool isUpdate)
        {
            var patchResult = await PatchingProcess(isUpdate);

            textTest = "listooooo!";

            if (patchResult.Item1 > 0)
            {
                RestartWindow();

                switch (patchResult.Item1)
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
                    case (int)ErrorCodes.HashError:
                        PlaySource = $"{assetsImgPath}/ventana_error_hash.png";
                        cacheVisible = true;
                        break;
                }

                if (patchResult.Item2 != string.Empty)
                {
                    aceptarVisible = true;
                    var errorLogFile = $"{Directory.GetCurrentDirectory()}{Path.DirectorySeparatorChar}error.log";
                    File.WriteAllText(errorLogFile, patchResult.Item2);
                }
            }
            else
            {
                if (!isUpdate)
                {
                    patchingVisible = false;
                    aceptarVisible = true;
                    PlaySource = $"{assetsImgPath}/ventana_exito.png";
                }
                else
                {
                    Process.Start("explorer", "steam://rungameid/787480");

                    await Dispatcher.UIThread.InvokeAsync(Close);
                }
            }
        }

        public Action Close { get; set; }

        private void RestartButtons()
        {
            errorVisible = false;
            patchVisible = false;
            patchingVisible = false;
            aceptarVisible = false;
            cacheVisible = false;
        }

        private void RestartWindow()
        {
            errorVisible = false;
            patchVisible = false;
            patchingVisible = false;
            cacheVisible = false;
            PlaySource = $"{assetsImgPath}/ventana.png";
        }

        public enum ErrorCodes : int
        {
            DownloadError = 1,
            ExtractError = 2,
            PatchError = 3,
            HashError = 4
        }
    }

    interface ICloseWindows
    {
        Action Close { get; set; }
    }
}

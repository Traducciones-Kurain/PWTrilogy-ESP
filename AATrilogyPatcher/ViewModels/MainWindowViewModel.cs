using Avalonia.Threading;
using System;
using System.Diagnostics;
using System.Linq;
using System.IO;
using System.Reactive;
using System.Threading.Tasks;
using Microsoft.Win32;
using ReactiveUI;
using System.Runtime.InteropServices;

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

        private string _progressText = "";
        public string progressText
        {
            get => _progressText;
            set => this.RaiseAndSetIfChanged(ref _progressText, value);
        }

        private string _totalText = "";
        public string totalText
        {
            get => _totalText;
            set => this.RaiseAndSetIfChanged(ref _totalText, value);
        }

        private bool _textVisible;
        public bool textVisible
        {
            get => _textVisible;
            set => this.RaiseAndSetIfChanged(ref _textVisible, value);
        }

        private bool _textVisibleApl;
        public bool textVisibleApl
        {
            get => _textVisibleApl;
            set => this.RaiseAndSetIfChanged(ref _textVisibleApl, value);
        }

        private bool _updateWindow;
        public bool updateWindow
        {
            get => _updateWindow;
            set => this.RaiseAndSetIfChanged(ref _updateWindow, value);
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

            textVisible = false;
            textVisibleApl = false;

            updateWindow = false;

            PlaySource = $"{assetsImgPath}/ventana.png";
            steamPath = "";

            findPath = ReactiveCommand.Create(FindPath);
            startPatch = ReactiveCommand.CreateFromTask(StartPatch);
            restartButtons = ReactiveCommand.Create(RestartButtons);
        }

        void FindPath()
        {
            bool success = false;

            if (RuntimeInformation.IsOSPlatform(OSPlatform.Windows))
            {
                success = SteamKeyExists();
            }
            else if (RuntimeInformation.IsOSPlatform(OSPlatform.Linux))
            {
                // basado en https://github.com/TraduSquare/Mara/blob/main/Mara.Lib/Common/Steam/SteamUtils.cs#L34
                // y https://github.com/TraduSquare/Mara/blob/main/Mara.Lib/Common/Steam/Api/LibraryFolders.cs#L11
                // TODO: revisar esto y hacer que sea un poco mas amable a la vista
                var home = Environment.GetFolderPath(Environment.SpecialFolder.UserProfile);
                var paths = new[] { ".steam", ".steam/steam", ".steam/root", ".local/share/Steam",
                ".var/app/com.valvesoftware.Steam/.steam", ".var/app/com.valvesoftware.Steam/.steam/steam", ".var/app/com.valvesoftware.Steam/.steam/root"}; // flatpak

                var steamPaths = paths
                    .Select(path => Path.Join(home, path))
                    .Where(steamPath => Directory.Exists(Path.Join(steamPath, "appcache")));

                if (steamPaths.Any())
                {
                    var libraryfolders = Path.Combine(steamPaths.First(), "config", "libraryfolders.vdf");

                    if (File.Exists(libraryfolders))
                    {
                        var txt = File.ReadAllLines(libraryfolders);

                        foreach (var s in txt)
                        {
                            if (s.Contains("\"path\""))
                            {
                                var library = s.Replace("		\"path\"		\"", "").Replace("\"", "");

                                var tmpPath = Path.Combine(library, "steamapps", "common", "Phoenix Wright Ace Attorney Trilogy");
                                if (Directory.Exists(tmpPath))
                                {
                                    steamGamePath = tmpPath;
                                    success = true;
                                }
                            }
                        }
                    }
                }
            }

            if (success)
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
            updateWindow = true;

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
            PlaySource = $"{assetsImgPath}/ventana_apl_descarga.png";

            // basado en CheckGame(), https://github.com/Darkmet98/OkamiPatcher/blob/main/OkamiPatcher/Controllers/SteamController.cs#L95
            var files = new string[]
            {
                "PWAAT.exe",
                "UnityPlayer.dll",
            };

            if (files.Any(file => !File.Exists($"{steamPath}{Path.DirectorySeparatorChar}{file}")))
            {
                return ((int)ErrorCodes.FileError, string.Empty);
            }

            var assemblyDll = $"{steamPath}{Path.DirectorySeparatorChar}PWAAT_Data{Path.DirectorySeparatorChar}Managed{Path.DirectorySeparatorChar}Assembly-CSharp.dll";

            if (!File.Exists(assemblyDll))
            {
                if (!File.Exists(assemblyDll + "_ori"))
                    return ((int)ErrorCodes.FileError, string.Empty);
            }

            if (!Directory.Exists($"{steamPath}{Path.DirectorySeparatorChar}PWAAT_Data{Path.DirectorySeparatorChar}StreamingAssets{Path.DirectorySeparatorChar}InternationalFiles"))
                return ((int)ErrorCodes.FileError, string.Empty);

            textVisible = true;
            progressText = "0";
            totalText = "0";

            string downloadPath = $"{Directory.GetCurrentDirectory()}{Path.DirectorySeparatorChar}Patch-Steam.zip";
            var patchProcess = new Patch.PatchAsync(steamPath, downloadPath);

            var progress = new Progress<(string, string)>(value =>
            {
                progressText = value.Item1;
                totalText = value.Item2;
            });

            if (!File.Exists(downloadPath))
            {
                var downloadPatch = await patchProcess.DownloadPatch(progress);
                if (!downloadPatch.Item1)
                {
                    return ((int)ErrorCodes.DownloadError, downloadPatch.Item2);
                }
            }

            textVisible = false;
            progressText = "0";
            totalText = "0";

            PlaySource = $"{assetsImgPath}/ventana_apl_extraer.png";

            var extractPatch = await patchProcess.ExtractPatch();
            if (!extractPatch.Item1)
            {
                patchProcess.patchProcess.DeleteTempFolder();
                return ((int)ErrorCodes.ExtractError, extractPatch.Item2);
            }

            PlaySource = $"{assetsImgPath}/ventana_apl.png";
            textVisibleApl = true;

            var patchGame = await patchProcess.PatchGame(progress);
            if (patchGame.Item1 > 0 && patchGame.Item1 != 1)
            {
                patchProcess.patchProcess.DeleteTempFolder();
                return ((int)ErrorCodes.PatchError, patchGame.Item2);
            }
            else if (patchGame.Item1 == 1)
            {
                return ((int)ErrorCodes.HashError, string.Empty);
            }

            File.Delete(downloadPath);

            return (0, string.Empty);
        }

        async Task StartPatchingProcess(bool isUpdate)
        {
            var patchResult = await PatchingProcess(isUpdate);

            updateWindow = false;

            textVisible = false;
            textVisibleApl = false;
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
                else
                {
                    if (patchResult.Item1 == (int)ErrorCodes.FileError)
                    {
                        PlaySource = $"{assetsImgPath}/ventana_error_archivos.png";
                        aceptarVisible = true;
                    }
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
            HashError = 4,
            FileError = 5
        }
    }

    interface ICloseWindows
    {
        Action Close { get; set; }
    }
}

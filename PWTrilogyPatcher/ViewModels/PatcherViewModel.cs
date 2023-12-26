using Avalonia.Platform.Storage;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Microsoft.Extensions.DependencyInjection;
using PWTrilogyPatcher.CustomMara;
using PWTrilogyPatcher.Services;
using System;
using System.Diagnostics;
using System.IO;
using System.Net;
using System.Threading.Tasks;

namespace PWTrilogyPatcher.ViewModels;

public enum PatcherStatus
{
    NoFolder,
    InvalidFolder,
    ValidFolder,
    Downloading,
    Patching,
    PatchError,
    PatchSuccess
}

public enum GamePlatform
{
    Steam,
    Xbox
}

public partial class PatcherViewModel : ViewModelBase
{
    public string PathWatermark => "Introduce la ruta del juego aquí";
    public const string HeaderStatusConst = "Elige la ruta de juego:";

    public const string StatusNoFolder = "Introduce la ruta de juego.";
    public const string StatusInvalidFolder = "Ruta de juego inválida";
    public const string StatusDetectedFolder = "Ruta de juego detectada.";
    public const string StatusValidFolderLine1 = "Plataforma detectada: ";
    public const string StatusValidFolderLine2 = "Listo para aplicar.";
    public const string StatusDownloading = "Descargando el parche...";
    public const string StatusPatching = "Aplicando el parche...";
    public const string StatusPatchError1 = $"Error al aplicar el parche.";
    public const string StatusPatchError2 = $"Revisa el archivo error.log";
    public const string StatusPatchError3 = $"Si necesitas soporte técnico, visita el servidor";
    public const string StatusPatchError4 = $"de Discord de Traducciones Kurain.";
    public const string StatusPatchSuccess = "¡Se ha parcheado el juego con éxito!";

    private static bool didForcedUpdate = false;

    public PatcherStatus PatcherStatus
    {
        get => patcherStatus;
        set
        {
            patcherStatus = value;

            switch (patcherStatus)
            {
                default:
                case PatcherStatus.NoFolder:
                    Status = StatusNoFolder;
                    break;
                case PatcherStatus.InvalidFolder:
                    Status = StatusInvalidFolder;
                    break;
                case PatcherStatus.ValidFolder:
                    Status = $"{StatusValidFolderLine1}{GetPlatformString()}{Environment.NewLine}{StatusValidFolderLine2}";
                    break;
                case PatcherStatus.Downloading:
                    Status = StatusDownloading;
                    break;
                case PatcherStatus.Patching:
                    Status = StatusPatching;
                    break;
                case PatcherStatus.PatchError:
                    Status = $"{StatusPatchError1}{Environment.NewLine}{StatusPatchError2}{Environment.NewLine}{Environment.NewLine}{StatusPatchError3}{Environment.NewLine}{StatusPatchError4}";
                    break;
                case PatcherStatus.PatchSuccess:
                    Status = StatusPatchSuccess;
                    break;
            }
        }
    }

    public string GamePath
    {
        get => gamePath;
        set
        {
            this.SetProperty(ref gamePath, value);

            if (string.IsNullOrEmpty(value))
            {
                PatcherStatus = PatcherStatus.NoFolder;
                CanPatch = false;
                return;
            }

            if (CheckValidGameFolder(value, true))
            {
                PatcherStatus = PatcherStatus.ValidFolder;
                CanPatch = true;
                return;
            }

            PatcherStatus = PatcherStatus.InvalidFolder;
            CanPatch = false;
        }
    }
    
    public string ProgressText
    {
        get => progressText;
        set
        {
            this.SetProperty(ref progressText, value);
            UpdateProgress();
        }
    }

    public string TotalText
    {
        get => totalText;
        set
        {
            this.SetProperty(ref totalText, value);
            UpdateProgress();
        }
    }

    private PatcherStatus patcherStatus;
    private string gamePath = string.Empty;
    private string progressText = string.Empty;
    private string totalText = string.Empty;
    private bool updateMode = false;

    [ObservableProperty]
    private string status = string.Empty;

    [ObservableProperty]
    private bool canPatch;

    [ObservableProperty]
    private bool startedPatch;

    [ObservableProperty]
    private GamePlatform gamePlatform;

    [ObservableProperty]
    private string headerStatus = string.Empty;

    public PatcherViewModel()
    {
        bool detectedPath = false;
        updateMode = App.UpdateMode;

        HeaderStatus = HeaderStatusConst;

        if (updateMode)
        {
            GamePath = Directory.GetCurrentDirectory();

            if (!didForcedUpdate)
            {
                didForcedUpdate = true;
                StartPatch();
                return;
            }

            detectedPath = true;
        }
        else
        {
            var steamPath = SteamUtils.GetSteamPath();
            if (!string.IsNullOrWhiteSpace(steamPath))
            {
                var gamePath = SteamUtils.GetSteamGamePath(steamPath);
                if (!string.IsNullOrWhiteSpace(gamePath))
                {
                    GamePath = gamePath;
                    detectedPath = true;
                }
            }
        }

        if (detectedPath)
        {
            Status = $"{StatusDetectedFolder}{Environment.NewLine}{Environment.NewLine}{Status}";
            return;
        }

        PatcherStatus = PatcherStatus.NoFolder;
    }

    [RelayCommand]
    private async Task OpenFolder()
    {
        var storageService = App.Current?.Services?.GetService<IStorageService>();
        if (storageService is null) return; // throw new NullReferenceException("Missing File Service instance.")

        var folder = await storageService.OpenFolderAsync();
        if (folder is null) return;

        GamePath = folder.TryGetLocalPath();
    }

    [RelayCommand]
    private async Task StartPatch()
    {
        CanPatch = false;
        StartedPatch = true;

        var platform = GetPlatformString();
        string downloadPath = $"{Directory.GetCurrentDirectory()}{Path.DirectorySeparatorChar}Patch-{platform}.zip";

        try
        {
            ProgressText = string.Empty;
            TotalText = string.Empty;

            var progress = new Progress<(string, string)>(value =>
            {
                ProgressText = value.Item1;
                TotalText = value.Item2;
            });

            if (!File.Exists(downloadPath))
            {
                PatcherStatus = PatcherStatus.Downloading;

                var downloadUrl = $"https://github.com/Traducciones-Kurain/PWTrilogy-ESP/releases/latest/download/Patch-{platform}.zip";
                await GetFileAsync(downloadUrl, downloadPath, progress);
            }

            PatcherStatus = PatcherStatus.Patching;

            var gamePath = GetContentFolder(GamePath);
            var patchProcess = new PatchProcessAsync(gamePath, gamePath, downloadPath);

            var result = await patchProcess.ApplyTranslation(progress);
            if (result.Item1 != 0)
            {
                throw new Exception($"[Mara] {result.Item2}");
            }

            if (updateMode)
            {
                Process.Start("explorer", "steam://rungameid/787480");
                App.Current?.Shutdown();
            }

            PatcherStatus = PatcherStatus.PatchSuccess;
        }
        catch (Exception e)
        {
            PatcherStatus = PatcherStatus.PatchError;

            var errorLogFile = $"{Directory.GetCurrentDirectory()}{Path.DirectorySeparatorChar}error.log";
            File.WriteAllText(errorLogFile, $"{e.Message}{Environment.NewLine}{e.StackTrace}");
        }
        finally
        {
            if (File.Exists(downloadPath))
            {
                File.Delete(downloadPath);
            }

            CanPatch = true;
        }
    }

    private void UpdateDownloadProgress()
    {
        Status = $"{StatusDownloading} ({ProgressText} de {TotalText} MB)";
    }

    private void UpdatePatchProgress()
    {
        Status = $"{StatusPatching} ({ProgressText} de {TotalText} archivos)";
    }

    private void UpdateProgress()
    {
        if (PatcherStatus == PatcherStatus.Downloading)
        {
            UpdateDownloadProgress();
        }

        if (PatcherStatus == PatcherStatus.Patching)
        {
            UpdatePatchProgress();
        }
    }

    private string GetPlatformString()
    {
        switch(GamePlatform)
        {
            case GamePlatform.Steam:
                return "Steam";
            case GamePlatform.Xbox:
                return "Xbox";
        }

        return string.Empty;
    }

    private bool CheckRootFiles(string path)
    {
        var exePath = Path.Combine(path, "PWAAT.exe");
        if (!File.Exists(exePath)) return false;

        var dataPath = Path.Combine(path, "PWAAT_Data");
        if (!Directory.Exists(dataPath)) return false;

        return true;
    }

    private string GetContentFolder(string path)
    {
        var contentPath = Path.Combine(path, "Content");

        if (Directory.Exists(contentPath) && CheckRootFiles(contentPath))
        {
            return contentPath;
        }

        return path;
    }

    private bool CheckValidGameFolder(string path, bool setGamePlatform)
    {
        var gamePath = GetContentFolder(path);

        if (!CheckRootFiles(gamePath)) return false;

        var dataPath = Path.Combine(gamePath, "PWAAT_Data");

        var steamAssemblyPath = Path.Combine(dataPath, "Managed", "Assembly-CSharp.dll");
        if (File.Exists(steamAssemblyPath))
        {
            if (setGamePlatform)
            {
                GamePlatform = GamePlatform.Steam;
            }

            return true;
        }

        var metadataPath = Path.Combine(dataPath, "il2cpp_data", "Metadata", "global-metadata.dat");
        if (File.Exists(metadataPath))
        {
            if (setGamePlatform)
            {
                GamePlatform = GamePlatform.Xbox;
            }

            return true;
        }

        return false;
    }

    // https://github.com/TraduSquare/Mara/blob/b3518f380786b1cd5a14e7b60168c42bdce1623c/Mara.Lib/Common/Internet.cs
    private async Task GetFileAsync(string url, string path, IProgress<(string, string)> progress)
    {
        var random = new Random();
        url += $"?random={random.Next()}";
        using var client = new WebClient();
        client.DownloadProgressChanged += (s, e) => progress.Report(($"{decimal.Truncate(decimal.Divide(e.BytesReceived, 1048576))}", $"{decimal.Truncate(decimal.Divide(e.TotalBytesToReceive, 1048576))}"));
        await client.DownloadFileTaskAsync(url, path);
    }
}

public class DesignPatcherViewModel : PatcherViewModel
{
    public DesignPatcherViewModel() : base()
    {

    }
}

public class DesignPatcherViewModelDownloading : PatcherViewModel
{
    public DesignPatcherViewModelDownloading() : base()
    {
        CanPatch = false;
        StartedPatch = true;

        PatcherStatus = PatcherStatus.Downloading;
    }
}

public class DesignPatcherViewModelPatchingDone : PatcherViewModel
{
    public DesignPatcherViewModelPatchingDone() : base()
    {
        CanPatch = true;
        StartedPatch = true;

        PatcherStatus = PatcherStatus.PatchSuccess;
    }
}

public class DesignPatcherViewModelPatchingError : PatcherViewModel
{
    public DesignPatcherViewModelPatchingError() : base()
    {
        CanPatch = true;
        StartedPatch = true;

        PatcherStatus = PatcherStatus.PatchError;
    }
}

using System;
using System.Collections.Generic;
using System.IO;
using System.Net;
using Mara.Lib.Common;
using AATrilogyPatcherSteam.ViewModels;

namespace AATrilogyPatcherSteam.Patch
{
    // version modificada de https://github.com/TraduSquare/Mara/blob/main/Mara.Lib/Platforms/Generic/Main.cs debido a que el parche crea archivos nuevos
    public class Main : Mara.Lib.Platforms.PatchProcess
    {
        private List<string> replacedFiles = new List<string>();

        public Main(string oriFolder, string outFolder, string filePath) : base(oriFolder, outFolder, filePath)
        {
        }

        public override (int, string) ApplyTranslation()
        {
            var count = maraConfig.FilesInfo.ListOriFiles.Length;
            var files = maraConfig.FilesInfo;

            for (int i = 0; i < count; i++)
            {
                var excludeFile = false;

                if (files.ListExcludeFiles != null)
                    excludeFile = CheckExcludeFile(files.ListOriFiles[i]);

                var oriFile = files.ListOriFiles[i];
                var xdeltaFile = files.ListXdeltaFiles[i];
                var outFile = xdeltaFile.Substring(0, xdeltaFile.Length - 7);

                if (excludeFile)
                    if (!File.Exists(oriFile))
                        continue;

                var result = ApplyXdelta(oriFile, xdeltaFile, outFile, files.ListMd5Files[i]);

                if (result.Item1 != 0)
                    return result;
            }

            return base.ApplyTranslation();
        }

        private bool CheckExcludeFile(string file)
        {
            foreach (var excludeFile in maraConfig.FilesInfo.ListExcludeFiles)
            {
                if (excludeFile == file)
                    return true;
            }

            return false;
        }

        private new(int, string) ApplyXdelta(string file, string xdelta, string result, string md5)
        {
            var oriFile = $"{oriFolder}{Path.DirectorySeparatorChar}{file}";
            var xdeltaFile = $"{tempFolder}{Path.DirectorySeparatorChar}{xdelta}";
            var outFile = $"{oriFolder}{Path.DirectorySeparatorChar}{result}";

            if (file == result)
            {
                if (!File.Exists(oriFile + "_ori"))
                    File.Move(oriFile, oriFile + "_ori");

                if (File.Exists(oriFile))
                    File.Delete(oriFile);

                System.Diagnostics.Debug.WriteLine($"added to list {file}");

                replacedFiles.Add(file);

                oriFile += "_ori";
            }
            else
            {
                if (replacedFiles.Contains(file))
                    oriFile += "_ori";

                //System.Diagnostics.Debug.WriteLine($"test123 {oriFile}");
            }

            if (Md5.CalculateMd5(oriFile) != md5)
                return (1, $"The file \"{oriFile}\" is not equal than the original file.");

            var outdir = Path.GetDirectoryName(outFile);
            if (!Directory.Exists(outdir))
                Directory.CreateDirectory(outdir);

            try
            {
                Xdelta.Apply(File.Open(oriFile, FileMode.Open), File.ReadAllBytes(xdeltaFile), outFile);
            }
            catch (Exception e)
            {
                return (99, $"Error patching the file.\n\n{e.Message}");
            }

            return (0, string.Empty);
        }
    }

    class Patch
    {
        // basado en https://github.com/Darkmet98/OkamiPatcher/blob/main/OkamiPatcher/Controllers/SteamController.cs

        private static string steamPath = MainWindowViewModel.steamGamePath;
        private static string url = "https://github.com/Traducciones-Kurain/AATrilogy-2019-ESP/releases/latest/download/Patch-Steam.zip";
        private static Main patchProcess;
        private static string downloadPath = $"{Directory.GetCurrentDirectory()}{Path.DirectorySeparatorChar}Patch-Steam.zip";
        public static (int, string) PatchProcess()
        {
            if (!File.Exists(downloadPath))
            {
                var downloadPatch = DownloadPatch();
                if (!downloadPatch.Item1)
                {
                    return ((int)MainWindowViewModel.ErrorCodes.DownloadError, downloadPatch.Item2);
                }
            }

            var extractPatch = ExtractPatch();
            if (!extractPatch.Item1)
            {
                return ((int)MainWindowViewModel.ErrorCodes.ExtractError, extractPatch.Item2);
            }

            var patchGame = PatchGame();
            if (!patchGame.Item1)
            {
                return ((int)MainWindowViewModel.ErrorCodes.PatchError, patchGame.Item2);
            }

            return (0, string.Empty);
        }

        public static (bool, string) DownloadPatch()
        {
            try
            {
                if (File.Exists(downloadPath))
                    File.Delete(downloadPath);

                Internet.GetFile(url, downloadPath);
            }
            catch (Exception e)
            {
                return (false, $"Se ha producido un error descargando los archivos.\n{e.Message}\n{e.StackTrace}");
            }
            return (true, string.Empty);
        }

        public static (bool, string) ExtractPatch()
        {
            try
            {
                patchProcess = new Main(steamPath, steamPath, downloadPath);
            }
            catch (Exception e)
            {
                return (false, $"Se ha producido un error extrayendo los archivos.\n{e.Message}\n{e.StackTrace}");
            }
            return (true, string.Empty);
        }

        public static (bool, string) PatchGame()
        {
            try
            {
                var result = patchProcess.ApplyTranslation();
                if (result.Item1 == 0)
                    return (true, string.Empty);
                return (false, $"Se ha producido un error aplicando la traducción.\nError: {result.Item1}\nMensaje: {result.Item2}");
            }
            catch (Exception e)
            {
                return (false, $"Se ha producido un error aplicando la traducción.\nError: INTERNAL CRASH\nMensaje:\n{e.Message}\n{e.StackTrace}");
            }
        }
    }
}

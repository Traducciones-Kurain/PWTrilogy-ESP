using System;
using System.Collections.Generic;
using System.Security.Cryptography;
using System.IO;
using System.Net;
using System.Threading.Tasks;
using Mara.Lib;
using Mara.Lib.Common;
using Mara.Lib.Configs;
using Newtonsoft.Json;
using AATrilogyPatcher.ViewModels;

namespace AATrilogyPatcher.Patch
{
    // version modificada de https://github.com/TraduSquare/Mara/blob/main/Mara.Lib/Platforms/PatchProcess.cs y https://github.com/TraduSquare/Mara/blob/main/Mara.Lib/Platforms/Generic/Main.cs debido a que el parche crea archivos nuevos

    public class PatchProcessAsync
    {
        protected MaraConfig maraConfig;
        protected string tempFolder;
        protected string oriFolder;
        protected string outFolder;
        protected string filePath;

        private List<string> replacedFiles = new List<string>();

        public PatchProcessAsync(string oriFolder, string outFolder, string filePath)
        {
            this.oriFolder = oriFolder;
            this.outFolder = outFolder;
            this.filePath = filePath;
        }

        public async Task<(int, string)> ApplyTranslation(IProgress<(string, string)> progress)
        {
            var count = maraConfig.FilesInfo.ListOriFiles.Length;
            var files = maraConfig.FilesInfo;

            for (int i = 0; i < count; i++)
            {
                var excludeFile = false;

                if (files.ListExcludeFiles != null)
                    excludeFile = CheckExcludeFile(files.ListOriFiles[i]);

                var oriFile = files.ListOriFiles[i].Replace('\\', Path.DirectorySeparatorChar);
                var xdeltaFile = files.ListXdeltaFiles[i].Replace('\\', Path.DirectorySeparatorChar);
                var outFile = xdeltaFile.Substring(0, xdeltaFile.Length - 7).Replace('\\', Path.DirectorySeparatorChar);

                if (excludeFile)
                    if (!File.Exists(oriFile))
                        continue;

                var result = await ApplyXdelta(oriFile, xdeltaFile, outFile, files.ListMd5Files[i]);

                if (result.Item1 != 0)
                    return result;

                progress.Report((i.ToString(), count.ToString()));
            }

            Directory.Delete(tempFolder, true);
            return (0, string.Empty);
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

        private async Task<(int, string)> ApplyXdelta(string file, string xdelta, string result, string md5)
        {
            var oriFile = $"{oriFolder}{Path.DirectorySeparatorChar}{file}";
            var xdeltaFile = $"{tempFolder}{Path.DirectorySeparatorChar}{xdelta}";
            var outFile = $"{oriFolder}{Path.DirectorySeparatorChar}{result}";

            if (file == result)
            {
                if (!File.Exists(oriFile + "_ori"))
                    File.Copy(oriFile, oriFile + "_ori");

                var hash = await CalculateMd5(oriFile + "_ori");

                if (hash != md5)
                {
                    File.Delete(oriFile + "_ori");
                    return (1, $"The file \"{oriFile}\" is not equal than the original file.");
                }

                if (File.Exists(oriFile))
                    File.Delete(oriFile);

                replacedFiles.Add(file);

                oriFile += "_ori";
            }
            else
            {
                if (replacedFiles.Contains(file))
                    oriFile += "_ori";

                var hash = await CalculateMd5(oriFile);

                if (hash != md5)
                {
                    return (1, $"The file \"{oriFile}\" is not equal than the original file.");
                }
            }

            var outdir = Path.GetDirectoryName(outFile);
            if (!Directory.Exists(outdir))
                Directory.CreateDirectory(outdir);

            try
            {
                await Task.Run(() => Xdelta.Apply(File.Open(oriFile, FileMode.Open), File.ReadAllBytes(xdeltaFile), outFile));
            }
            catch (Exception e)
            {
                return (99, $"Error patching the file.\n\n{e.Message}");
            }

            return (0, string.Empty);
        }

        //https://github.com/TraduSquare/Mara/blob/main/Mara.Lib/Common/Md5.cs
        private async Task<string> CalculateMd5(string filename)
        {
            using (var md5 = MD5.Create())
            {
                using (var stream = new FileStream(filename, FileMode.Open, FileAccess.Read, FileShare.Read, 4096, true))
                {
                    byte[] buffer = new byte[4096];
                    int bytesRead;
                    do
                    {
                        bytesRead = await stream.ReadAsync(buffer, 0, 4096);
                        if (bytesRead > 0)
                        {
                            md5.TransformBlock(buffer, 0, bytesRead, null, 0);
                        }
                    } while (bytesRead > 0);

                    md5.TransformFinalBlock(buffer, 0, 0);
                    return BitConverter.ToString(md5.Hash).Replace("-", "").ToLowerInvariant();
                }
            }
        }

        public void GenerateTempFolder()
        {
            tempFolder = Path.GetTempPath() + Path.DirectorySeparatorChar + Path.GetRandomFileName();
            Directory.CreateDirectory(tempFolder);
        }

        public async Task ExtractPatch()
        {
            await Task.Run(() => Lzma.Unpack(filePath, tempFolder));

            var configText = await File.ReadAllTextAsync($"{tempFolder}{Path.DirectorySeparatorChar}data.json");
            maraConfig = JsonConvert.DeserializeObject<MaraConfig>(configText);
        }

        public void DeleteTempFolder()
        {
            if (Directory.Exists(tempFolder))
                Directory.Delete(tempFolder, true);
        }

        public PatchInfo GetInfo()
        {
            return maraConfig.Info;
        }
    }

    // basado en https://github.com/Darkmet98/OkamiPatcher/blob/main/OkamiPatcher/Controllers/SteamController.cs
    class PatchAsync
    {
        private string steamPath;
        private string downloadPath;
        private string url = "https://github.com/Traducciones-Kurain/AATrilogy-2019-ESP/releases/latest/download/Patch-Steam.zip";

        public PatchProcessAsync patchProcess;

        public PatchAsync(string steamPath, string downloadPath)
        {
            this.steamPath = steamPath;
            this.downloadPath = downloadPath;
        }

        //https://github.com/TraduSquare/Mara/blob/main/Mara.Lib/Common/Internet.cs
        private async Task GetFileAsync(string url, string path, IProgress<(string, string)> progress)
        {
            var random = new Random();
            url += $"?random={random.Next()}";
            using var client = new WebClient();
            client.DownloadProgressChanged += (s, e) => progress.Report(($"{decimal.Truncate(decimal.Divide(e.BytesReceived, 1048576))}", $"{decimal.Truncate(decimal.Divide(e.TotalBytesToReceive, 1048576))}"));
            await client.DownloadFileTaskAsync(url, path);
        }

        public async Task<(bool, string)> DownloadPatch(IProgress<(string, string)> progress)
        {
            try
            {
                if (File.Exists(downloadPath))
                    File.Delete(downloadPath);

                await GetFileAsync(url, downloadPath, progress);
            }
            catch (Exception e)
            {
                if (File.Exists(downloadPath))
                    File.Delete(downloadPath);

                return (false, $"Se ha producido un error descargando los archivos.\n{e.Message}\n{e.StackTrace}");
            }
            return (true, string.Empty);
        }

        public async Task<(bool, string)> ExtractPatch()
        {
            try
            {
                patchProcess = new PatchProcessAsync(steamPath, steamPath, downloadPath);
                patchProcess.GenerateTempFolder();
                await patchProcess.ExtractPatch();
            }
            catch (Exception e)
            {
                return (false, $"Se ha producido un error extrayendo los archivos.\n{e.Message}\n{e.StackTrace}");
            }
            return (true, string.Empty);
        }

        public async Task<(int, string)> PatchGame(IProgress<(string, string)> progress)
        {
            try
            {
                var result = await patchProcess.ApplyTranslation(progress);
                if (result.Item1 == 0)
                {
                    return (0, string.Empty);
                }
                return (result.Item1, $"Se ha producido un error aplicando la traducción.\nError: {result.Item1}\nMensaje: {result.Item2}");
            }
            catch (FileNotFoundException e)
            {
                return (1, $"Se ha producido un error aplicando la traducción.\nError: INTERNAL CRASH\nMensaje:\n{e.Message}\n{e.StackTrace}");
            }
            catch (Exception e)
            {
                return (2, $"Se ha producido un error aplicando la traducción.\nError: INTERNAL CRASH\nMensaje:\n{e.Message}\n{e.StackTrace}");
            }
        }
    }
}

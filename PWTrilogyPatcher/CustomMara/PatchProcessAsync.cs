using Mara.Lib.Common;
using Mara.Lib.Configs;
using Mara.Lib;
using Newtonsoft.Json;
using System;
using System.IO;
using System.Threading.Tasks;

namespace PWTrilogyPatcher.CustomMara;

// based on https://github.com/TraduSquare/Mara/blob/0b0879ae54e373708f6729de6acf70c1a06399a8/Mara.Lib/Platforms/PatchProcess.cs
// and      https://github.com/TraduSquare/Mara/blob/0b0879ae54e373708f6729de6acf70c1a06399a8/Mara.Lib/Platforms/Generic/Main.cs
public class PatchProcessAsync
{
    private MaraConfig maraConfig;
    private string tempFolder;
    private string oriFolder;
    private string outFolder;
    private string filePath;

    public PatchProcessAsync(string oriFolder, string outFolder, string filePath)
    {
        this.oriFolder = oriFolder;
        this.outFolder = outFolder;
        this.filePath = filePath;
        GenerateTempFolder();
        ExtractPatch();
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
            var xdelta = files.ListXdeltaFiles[i].Replace('\\', Path.DirectorySeparatorChar);
            var outFile = xdelta.Substring(0, xdelta.Length - 7);

            if (excludeFile)
                if (!File.Exists(oriFile))
                    continue;

            var result = await Task.Run(() => ApplyXdelta(oriFile, xdelta, outFile, files.ListMd5Files[i]));

            if (result.Item1 != 0)
                return result;

            progress.Report((i.ToString(), count.ToString()));
        }

        DeleteTempFolder();
        return (0, string.Empty);
    }

    private (int, string) ApplyXdelta(string file, string xdelta, string result, string md5)
    {
        var oriFile = $"{oriFolder}{Path.DirectorySeparatorChar}{file}";
        var xdeltaFile = $"{tempFolder}{Path.DirectorySeparatorChar}{xdelta}";
        var outFile = $"{oriFolder}{Path.DirectorySeparatorChar}{result}";

        bool isNewFile = file != result;

        string oriPath = oriFile + "_ori";
        string md5File = File.Exists(oriPath) ? oriPath : oriFile;

        if (Md5.CalculateMd5(md5File) != md5)
            return (1, $"The file \"{oriFile}\" is not equal than the original file.");

        if (!File.Exists(oriPath))
        {
            if (isNewFile)
            {
                File.Copy(oriFile, oriPath);
            }
            else
            {
                File.Move(oriFile, oriPath);
            }
        }

        if (!isNewFile && File.Exists(oriFile))
            File.Delete(oriFile);

        var outdir = Path.GetDirectoryName(outFile);
        if (!string.IsNullOrWhiteSpace(outdir) && !Directory.Exists(outdir))
            Directory.CreateDirectory(outdir);

        try
        {
            Xdelta.Apply(File.Open(oriPath, FileMode.Open), File.ReadAllBytes(xdeltaFile), outFile);
        }
        catch (Exception e)
        {
            return (99, $"Error patching the file.\n\n{e.Message}");
        }

        return (0, string.Empty);
    }

    private void GenerateTempFolder()
    {
        tempFolder = Path.GetTempPath() + Path.DirectorySeparatorChar + Path.GetRandomFileName();
        Directory.CreateDirectory(tempFolder);
    }

    private void ExtractPatch()
    {
        Lzma.Unpack(filePath, tempFolder);
        maraConfig = JsonConvert.DeserializeObject<MaraConfig>(File.ReadAllText($"{tempFolder}{Path.DirectorySeparatorChar}data.json"));
    }

    private void DeleteTempFolder()
    {
        Directory.Delete(tempFolder, true);
    }

    public PatchInfo GetInfo()
    {
        return maraConfig.Info;
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
}

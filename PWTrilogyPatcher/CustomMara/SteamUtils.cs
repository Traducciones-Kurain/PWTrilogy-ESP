using Microsoft.Win32;
using System;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices;

namespace PWTrilogyPatcher.CustomMara;

public static class SteamUtils
{
    // https://github.com/TraduSquare/Mara/blob/b3518f380786b1cd5a14e7b60168c42bdce1623c/Mara.Lib/Common/Steam/SteamUtils.cs#L34
    public static string GetSteamPath()
    {
        if (RuntimeInformation.IsOSPlatform(OSPlatform.Linux))
        {
            var home = Environment.GetFolderPath(Environment.SpecialFolder.UserProfile);
            var paths = new[] { ".steam", ".steam/steam", ".steam/root", ".local/share/Steam",
                ".var/app/com.valvesoftware.Steam/.steam", ".var/app/com.valvesoftware.Steam/.steam/steam", ".var/app/com.valvesoftware.Steam/.steam/root"}; // flatpak

            var steamPaths = paths
                .Select(path => Path.Join(home, path))
                .Where(steamPath => Directory.Exists(Path.Join(steamPath, "appcache")));

            if (steamPaths.Any())
            {
                return steamPaths.First().Replace('\\', Path.DirectorySeparatorChar);
            }
            else
            {
                return string.Empty;
            }
        }

        if (RuntimeInformation.IsOSPlatform(OSPlatform.Windows))
        {
            var key = Registry.CurrentUser.OpenSubKey("SOFTWARE\\Valve\\Steam") ??
                      RegistryKey.OpenBaseKey(RegistryHive.LocalMachine, RegistryView.Registry64)
                          .OpenSubKey("SOFTWARE\\Valve\\Steam");

            if (key != null && key.GetValue("SteamPath") is string steamPath)
            {
                return steamPath.Replace('/', Path.DirectorySeparatorChar);
            }
            else
            {
                return string.Empty;
            }
        }

        throw new PlatformNotSupportedException();
    }

    // https://github.com/TraduSquare/Mara/blob/b3518f380786b1cd5a14e7b60168c42bdce1623c/Mara.Lib/Common/Steam/Api/LibraryFolders.cs#L11
    // https://github.com/TraduSquare/Mara/blob/b3518f380786b1cd5a14e7b60168c42bdce1623c/Mara.Lib/Common/Steam/SteamUtils.cs#L77
    public static string GetSteamGamePath(string steamPath)
    {
        var libraryfolders = Path.Combine(steamPath, "config", "libraryfolders.vdf");

        if (File.Exists(libraryfolders))
        {
            var txt = File.ReadAllLines(libraryfolders);

            foreach (var s in txt)
            {
                if (s.Contains("\"path\""))
                {
                    var library = s.Replace("		\"path\"		\"", "").Replace("\"", "").Replace("\\\\", "\\").Replace('\\', Path.DirectorySeparatorChar);

                    var tmpPath = Path.Combine(library, "steamapps", "common", "Phoenix Wright Ace Attorney Trilogy");
                    if (Directory.Exists(tmpPath))
                    {
                        return tmpPath;
                    }
                }
            }
        }

        return string.Empty;
    }
}

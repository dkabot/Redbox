using System;
using System.Runtime.InteropServices;
using Microsoft.Win32;
using Outerwall.Shell.Interop;

namespace Outerwall.Shell
{
    internal static class SystemParametersUtility
    {
        public static bool SetWallpaper(string wallpaper, WallpaperStyle style, TileWallpaper tile)
        {
            if (string.IsNullOrEmpty(wallpaper))
                throw new ArgumentNullException(nameof(wallpaper));
            if (!wallpaper.EndsWith(".bmp", StringComparison.OrdinalIgnoreCase))
                throw new ArgumentException(
                    string.Format("Only bitmap images can be set as a wallpaper. The specified image was \"{0}\".",
                        wallpaper), nameof(wallpaper));
            if (!SystemParametersInfo(SPI.SPI_SETDESKWALLPAPER, 0U, wallpaper,
                    SPIF.SPIF_UPDATEINIFILE | SPIF.SPIF_SENDCHANGE))
                return false;
            RegistryKey registryKey;
            try
            {
                registryKey = Registry.CurrentUser.OpenSubKey("Control Panel\\Desktop", true);
            }
            catch (Exception ex)
            {
                throw new ApplicationException("Unable to open desktop registry key.", ex);
            }

            if (registryKey == null)
                throw new ApplicationException("Unable to locate desktop registry key.");
            try
            {
                using (registryKey)
                {
                    registryKey.SetValue("WallpaperStyle", style);
                    registryKey.SetValue("TileWallpaper", tile);
                }
            }
            catch (Exception ex)
            {
                throw new ApplicationException("Unable to set wallpaper.", ex);
            }

            UpdatePerUserSystemParameters();
            return true;
        }

        [DllImport("user32.dll", SetLastError = true)]
        [return: MarshalAs(UnmanagedType.Bool)]
        private static extern bool SystemParametersInfo(
            SPI uiAction,
            uint uiParam,
            string pvParam,
            SPIF fWinIni);

        [DllImport("user32.dll")]
        private static extern bool UpdatePerUserSystemParameters();
    }
}
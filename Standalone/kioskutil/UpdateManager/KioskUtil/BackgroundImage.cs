using System.Runtime.InteropServices;
using Microsoft.Win32;

namespace Redbox.UpdateManager.KioskUtil
{
    public static class BackgroundImage
    {
        [DllImport("user32.dll", CharSet = CharSet.Auto)]
        private static extern int SystemParametersInfo(
            int uAction,
            int uParam,
            string lpvParam,
            int fuWinIni);

        public static void SetWallpaper(string wallPaperLocation)
        {
            SetWallpaper(wallPaperLocation, 2, 0);
        }

        public static string GetWallpaper()
        {
            var registryKey = Registry.CurrentUser.OpenSubKey("Control Panel\\Desktop", false);
            var wallpaper = registryKey.GetValue("WallPaper").ToString();
            registryKey.Close();
            return wallpaper;
        }

        private static void SetWallpaper(
            string WallpaperLocation,
            int WallpaperStyle,
            int TileWallpaper)
        {
            SystemParametersInfo(20, 0, WallpaperLocation, 3);
            var registryKey = Registry.CurrentUser.OpenSubKey("Control Panel\\Desktop", true);
            registryKey.SetValue(nameof(WallpaperStyle), WallpaperStyle);
            registryKey.SetValue(nameof(TileWallpaper), TileWallpaper);
            registryKey.Close();
        }
    }
}
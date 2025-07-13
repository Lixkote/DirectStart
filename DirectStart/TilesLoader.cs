using IWshRuntimeLibrary;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using Microsoft.Win32;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml.Linq;
using System.Runtime.InteropServices;
using File = System.IO.File;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows;
using System.Drawing;
using System.Windows.Interop;

namespace B8TAM
{
    class TilesLoader
    {
        [DllImport("shell32.dll", CharSet = CharSet.Auto)]
        private static extern IntPtr SHGetFileInfo(string pszPath, uint dwFileAttributes, out SHFILEINFO psfi, uint cbFileInfo, uint uFlags);

        [DllImport("user32.dll")]
        private static extern int DestroyIcon(IntPtr hIcon);

        private const uint SHGFI_SYSICONINDEX = 0x000004000;
        private const int SHIL_EXTRALARGE = 0x00000002;

        [StructLayout(LayoutKind.Sequential)]
        private struct SHFILEINFO
        {
            public IntPtr hIcon;
            public int iIcon;
            public uint dwAttributes;
            [MarshalAs(UnmanagedType.ByValTStr, SizeConst = 260)]
            public string szDisplayName;
            [MarshalAs(UnmanagedType.ByValTStr, SizeConst = 80)]
            public string szTypeName;
        }
        private const uint SHGFI_ICON = 0x000000100;

        [DllImport("shell32.dll", EntryPoint = "#727")]
        private static extern int SHGetImageList(int iImageList, ref Guid riid, ref IImageList ppv);
        private static Guid IID_IImageList = new Guid("46EB5926-582E-4017-9FDF-E8998DAA0950");

        [ComImport]
        [Guid("46EB5926-582E-4017-9FDF-E8998DAA0950")]
        [InterfaceType(ComInterfaceType.InterfaceIsIUnknown)]
        private interface IImageList
        {
            int Dummy(); // This method is just a placeholder.
            int GetIcon(int i, int flags, out IntPtr picon);
        }

        [DllImport("Shell32.dll", CharSet = CharSet.Auto)]
        extern static int SHDefExtractIcon(string pszIconFile, int iIndex, int uFlags, out IntPtr phiconLarge, IntPtr phiconSmall, int nIconSize);
        public static class NativeMethods
        {
            [DllImport("user32.dll", SetLastError = true)]
            [return: MarshalAs(UnmanagedType.Bool)]
            internal static extern bool DestroyIcon(IntPtr hIcon);
        }
        public static ImageSource ExtractArbitrarySizeIcon(string filePath, int size)
        {
            IntPtr hIcon;
            if (SHDefExtractIcon(filePath, 0, 0, out hIcon, IntPtr.Zero, 48) == 0)
            {
                try
                {
                    Icon extractedIcon = Icon.FromHandle(hIcon);
                    return Imaging.CreateBitmapSourceFromHIcon(
                        extractedIcon.Handle,
                        Int32Rect.Empty,
                        BitmapSizeOptions.FromEmptyOptions());
                }
                finally
                {
                    // Dispose the icon to prevent resource leaks
                    NativeMethods.DestroyIcon(hIcon);
                }
            }
            return null; // Failure
        }
        string GetShortcutTarget(string shortcutPath)
        {
            WshShell shell = new WshShell();
            IWshShortcut shortcut = (IWshShortcut)shell.CreateShortcut(shortcutPath);
            return shortcut.TargetPath;
        }
        public static ImageSource GetIcon(string fileName)
        {
            Icon icon = Icon.ExtractAssociatedIcon(fileName);
            return System.Windows.Interop.Imaging.CreateBitmapSourceFromHIcon(
                        icon.Handle,
                        new Int32Rect(0, 0, icon.Width, icon.Height),
                        BitmapSizeOptions.FromEmptyOptions());
        }
        public void LoadTileGroups(ObservableCollection<Tile> tilesCollection)
        {
            string configFile = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.UserProfile), "PinnedTilesDS.xml");

            if (!File.Exists(configFile))
            {
                Debug.WriteLine("(TilesLoader) PinnedTilesDS.xml not found. Creating a new one.");
                CreateEmptyPinnedTilesDS(configFile);
                return;
            }

            try
            {
                XDocument doc = XDocument.Load(configFile);

                foreach (XElement tileElement in doc.Descendants("Tile"))
                {
                    Tile tile = new Tile
                    {
                        Title = Path.GetFileNameWithoutExtension(tileElement.Element("Path")?.Value),
                        Path = tileElement.Element("Path")?.Value,
                        EXEPath = GetShortcutTarget(tileElement.Element("Path")?.Value),
                        PathMetro = tileElement.Element("PathMetro")?.Value,
                        Size = tileElement.Element("Size")?.Value,
                        Icon = ExtractArbitrarySizeIcon(GetShortcutTarget(tileElement.Element("Path")?.Value), 2),
                        IsLiveTileEnabled = bool.Parse(tileElement.Element("IsLiveTileEnabled")?.Value ?? "false"),
                        LeftGradient = TileColorCalculator.CalculateLeftGradient(IconHelper.GetLargeFileIcon(tileElement.Element("Path")?.Value), Path.GetFileNameWithoutExtension(tileElement.Element("Path")?.Value), tileElement.Element("TileColor")?.Value),
                        RightGradient = TileColorCalculator.CalculateRightGradient(IconHelper.GetLargeFileIcon(tileElement.Element("Path")?.Value), Path.GetFileNameWithoutExtension(tileElement.Element("Path")?.Value), tileElement.Element("TileColor")?.Value),
                        Border = TileColorCalculator.CalculateBorder(IconHelper.GetLargeFileIcon(tileElement.Element("Path")?.Value), Path.GetFileNameWithoutExtension(tileElement.Element("Path")?.Value), tileElement.Element("TileColor")?.Value),
                    };

                    tilesCollection.Add(tile);
                }
            }
            catch (Exception ex)
            {
                Debug.WriteLine("(TilesLoader) PinnedTilesDS.xml load error: " + ex.Message);
                Debug.WriteLine("(TilesLoader) Recreating empty PinnedTilesDS.xml...");
                CreateEmptyPinnedTilesDS(configFile);
            }
        }

        private void CreateEmptyPinnedTilesDS(string filePath)
        {
            try
            {
                XDocument emptyDoc = new XDocument(new XElement("Tiles"));
                emptyDoc.Save(filePath);
            }
            catch (Exception ex)
            {
                Debug.WriteLine("(TilesLoader) Failed to create default PinnedTilesDS.xml: " + ex.Message);
            }
        }


        private string GetFileLabel(string filePath)
        {
            if (File.Exists(filePath))
            {
                // If it's a shortcut (LNK file)
                if (Path.GetExtension(filePath).Equals(".lnk", StringComparison.OrdinalIgnoreCase))
                {
                    return GetShortcutLabel(filePath);
                }
                // If it's an executable (EXE file)
                else if (Path.GetExtension(filePath).Equals(".exe", StringComparison.OrdinalIgnoreCase))
                {
                    return GetExeLabel(filePath);
                }
                // Handle other file types if needed
            }

            return "(TilesLoader) File not found or unsupported type";
        }

        private string GetShortcutLabel(string shortcutPath)
        {
            try
            {
                WshShell shell = new WshShell();
                IWshShortcut shortcut = (IWshShortcut)shell.CreateShortcut(shortcutPath);
                return shortcut.TargetPath; // Shortcut label is the target path
            }
            catch (Exception ex)
            {
                return $"(TilesLoader) Error getting shortcut label: {ex.Message}";
            }
        }

        private string GetExeLabel(string exePath)
        {
            try
            {
                return FileVersionInfo.GetVersionInfo(exePath).FileDescription;
            }
            catch (Exception ex)
            {
                return $"(TilesLoader) Error getting executable label: {ex.Message}";
            }
        }
    }
}

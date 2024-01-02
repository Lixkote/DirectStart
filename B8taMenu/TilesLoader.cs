using IWshRuntimeLibrary;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml.Linq;
using File = System.IO.File;

namespace B8TAM
{
    class TilesLoader
    {
        public void LoadTileGroups(ObservableCollection<Tile> tilesCollection)
        {
            string configFile = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.UserProfile), "DirectStart", "Tiles", "Layout.xml");

            if (File.Exists(configFile))
            {
                try
                {
                    XDocument doc = XDocument.Load(configFile);

                    foreach (XElement tileElement in doc.Descendants("Tile"))
                    {
                        Tile tile = new Tile
                        {
                            Title = Path.GetFileNameWithoutExtension(tileElement.Element("Path")?.Value),
                            Path = tileElement.Element("Path")?.Value,
                            PathMetro = tileElement.Element("PathMetro")?.Value,
                            Size = tileElement.Element("Size")?.Value,
                            Icon = IconHelper.GetLargeFileIcon(tileElement.Element("Path")?.Value),
                            IsLiveTileEnabled = bool.Parse(tileElement.Element("IsLiveTileEnabled")?.Value ?? "false"),
                        };

                        tilesCollection.Add(tile);
                    }
                }
                catch (Exception ex)
                {
                    Debug.WriteLine("Error loading tiles Layout xml file " + ex.Message);
                }
            }
            else
            {
                Debug.WriteLine("Tiles layout xml file not found.");
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

            return "File not found or unsupported type";
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
                return $"Error getting shortcut label: {ex.Message}";
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
                return $"Error getting executable label: {ex.Message}";
            }
        }
    }
}

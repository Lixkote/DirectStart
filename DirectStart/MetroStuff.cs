using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Xml;
using System.Text;
using System.Runtime.InteropServices;
using B8TAM;

namespace B8TAM {
    public class MetroApp : StartMenuEntry
    {
        public string Name { get; set; }
        public string Path { get; set; }
        public string Icon { get; set; }
        public string Identity { get; set; }
    }

    public class MetroAppHelper
    {
        // Declare SHLoadIndirectString function from shlwapi.dll
        [DllImport("shlwapi.dll", CharSet = CharSet.Unicode)]
        public static extern int SHLoadIndirectString(
            string pszSource,
            StringBuilder pszOutBuf,
            int cchOutBuf,
            IntPtr pdwReserved);
        private static string GetName(string dir, string name, string identity)
        {
            StringBuilder sb = new StringBuilder();
            int result;

            result = SHLoadIndirectString(
                @"@{" + Path.GetFileName(dir) + "? ms-resource://" + identity + "/resources/" + name.Split(':')[1] + "}",
                sb, -1,
                IntPtr.Zero
            );

            if (result == 0) return sb.ToString();
            return "";
        }

        public static List<MetroApp> GetMetroApps(string directory)
        {
            List<MetroApp> metroApps = new List<MetroApp>();
            try
            {
                foreach (string dir in Directory.GetDirectories(directory))
                {
                    if (File.Exists(dir + @"\AppxManifest.xml"))
                    {
                        XmlDocument doc = new XmlDocument();
                        doc.Load(dir + @"\AppxManifest.xml");

                        // Skip Framework apps
                        if (doc.GetElementsByTagName("Framework")[0]?.InnerText.ToLower() == "true") continue;

                        // Extract the DisplayName and Identity
                        string name = doc.GetElementsByTagName("DisplayName")[0]?.InnerText ?? "";
                        string identity = doc.GetElementsByTagName("Identity")[0]?.Attributes["Name"]?.Value ?? "";

                        // Check if the name is already resolved (doesn't need to resolve ms-resource)
                        if (!name.Contains("ms-resource"))
                        {
                            metroApps.Add(new MetroApp
                            {
                                Name = name,
                                Path = dir,
                                Identity = identity,
                                Icon = GetIconFromManifest(doc, dir)
                            });
                        }
                        else
                        {
                            // Handle ms-resource names and resolve them
                            if (doc.GetElementsByTagName("Application").Count > 1)
                            {
                                foreach (XmlElement elem in doc.GetElementsByTagName("Application"))
                                {
                                    name = elem.GetElementsByTagName("m2:VisualElements")[0].Attributes["DisplayName"].Value;
                                    if (name.Contains("AppName")) name = name.Replace("AppName", "AppTitle");
                                    string resolvedName = GetName(dir, name, identity);
                                    if (!string.IsNullOrEmpty(resolvedName))
                                    {
                                        metroApps.Add(new MetroApp
                                        {
                                            Name = resolvedName,
                                            Path = dir,
                                            Identity = identity,
                                            Icon = GetIconFromManifest(doc, dir)
                                        });
                                    }
                                }
                            }
                            else
                            {
                                string resolvedName = GetName(dir, name, identity);
                                if (!string.IsNullOrEmpty(resolvedName))
                                {
                                    metroApps.Add(new MetroApp
                                    {
                                        Name = resolvedName,
                                        Identity = identity,
                                        Icon = GetIconFromManifest(doc, dir)
                                    });
                                }
                            }
                        }
                    }
                }

                return metroApps.Distinct().ToList();
            }
            catch
            {
                // System.Windows.MessageBox.Show("Ensure you have permissions to access WindowsApps folder", "We couldn't load metro apps support");
                List<MetroApp> emptylist = new List<MetroApp>();
                return emptylist;
            }
        }

        // Method to extract the icon from the AppxManifest.xml
        private static string GetIconFromManifest(XmlDocument doc, string dir)
        {
            try
            {
                string[] iconFiles = Directory.GetFiles(dir, "*targetsize-16*.png", SearchOption.AllDirectories);
                string iconPath = iconFiles[0]; // Assuming you want the first match;
                if (string.IsNullOrEmpty(iconFiles[0]) == true)
                {
                    string[] iconFiles1 = Directory.GetFiles(dir, "*logo*.png", SearchOption.AllDirectories);
                    string iconPath1 = iconFiles1[0]; // Assuming you want the first match;
                    return dir + @"\" + iconPath1;
                }
                else
                {
                    return iconPath ?? "";
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine("Error getting icon: " + ex.Message);
                return "";
            }
        }
    }
}

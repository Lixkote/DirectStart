using B8TAM;
using ControlzEx.Theming;
using Microsoft.Win32;
using System;
using System.Diagnostics;
using System.IO;
using System.IO.Pipes;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Threading;
using System.Windows;

namespace AFSM
{
    /// <summary>
    /// Interaction logic for App.xaml
    /// </summary>
    public partial class App : System.Windows.Application
    {
		static Mutex mutex = new Mutex(true, "{8f7112b5-18a2-4152-896c-97e0fb647681}");
		public App()
		{
			if (mutex.WaitOne(TimeSpan.Zero, true))
			{
				mutex.ReleaseMutex();
			}
			else
			{
				Current.Shutdown();
			}
		}
        private void HandleTriggerArgument()
        {
            using (var client = new NamedPipeClientStream(".", "DirectStartPipe", PipeDirection.Out))
            {
                try
                {
                    client.Connect();
                    using (var writer = new StreamWriter(client))
                    {
                        writer.WriteLine("TRIGGER");
                        writer.Flush();
                    }
                }
                catch (IOException ex)
                {
                    // Log or handle the error as needed
                    Debug.WriteLine("Named pipe error: " + ex.Message);
                }
            }
        }

        protected override void OnStartup(StartupEventArgs e)
        {
            base.OnStartup(e);

            string debug = "false";

            if (e.Args.Contains("/trigger") || debug == "true")
            {
                Environment.Exit(0);
            }
            else
            {
                try
                {
                    // Read values from the registry
                    RegistryKey key = Registry.CurrentUser.OpenSubKey(@"SOFTWARE\DirectStart");
                    if (key != null)
                    {
                        string theme = key.GetValue("theme") as string ?? "metro";
                        string profilePictureShape = key.GetValue("pfpshape") as string ?? "rounded";
                        string forceFillStartButton = key.GetValue("forcefillstartbutton") as string ?? "false";
                        string retrobarfix = key.GetValue("retrobarfix") as string ?? "false";
                        string noTilesBool = key.GetValue("disablenotiles") as string ?? "false";  // Assuming you meant "disabletiles"

                        // Apply theme
                        string resourceDictionaryPath = GetResourceDictionaryPath(theme);
                        if (!string.IsNullOrEmpty(resourceDictionaryPath))
                        {
                            ResourceDictionary skinDictionary = new ResourceDictionary
                            {
                                Source = new Uri(resourceDictionaryPath, UriKind.RelativeOrAbsolute)
                            };
                            Resources.MergedDictionaries.Add(skinDictionary);
                        }
                        else
                        {
                            MessageBox.Show("Invalid theme specified in the registry.");
                        }

                        this.Resources["ProfilePictureShape"] = profilePictureShape;
                        this.Resources["ForceFillStartButton"] = forceFillStartButton;
                        this.Resources["RetroBarFix"] = retrobarfix;
                        this.Resources["NoTilesBool"] = noTilesBool;

                        key.Close();
                    }
                    else
                    {
                        // Registry key doesn't exist, use defaults
                        ApplyDefaultSettings();
                    }
                }
                catch
                {
                    ApplyDefaultSettings();
                }

                SetLanguageDictionary();

                StartMenu mainWindow = new StartMenu();
                mainWindow.Show();
            }
        }
        private void ApplyDefaultSettings()
        {
            const string registryPath = @"SOFTWARE\DirectStart";
            using (RegistryKey key = Registry.CurrentUser.CreateSubKey(registryPath))
            {
                if (key != null)
                {
                    // Set default values if they do not exist
                    SetRegistryDefault(key, "pfpshape", "rounded");
                    SetRegistryDefault(key, "forcefillstartbutton", "false");
                    SetRegistryDefault(key, "retrobarfix", "false");
                    SetRegistryDefault(key, "disablenotiles", "false");
                    SetRegistryDefault(key, "theme", "metro");

                    // Set app resources using those values
                    this.Resources["ProfilePictureShape"] = key.GetValue("pfpshape") as string;
                    this.Resources["ForceFillStartButton"] = key.GetValue("forcefillstartbutton") as string;
                    this.Resources["RetroBarFix"] = key.GetValue("retrobarfix") as string;
                    this.Resources["NoTilesBool"] = key.GetValue("disablenotiles") as string;

                    string theme = key.GetValue("theme") as string;
                    string resourceDictionaryPath = GetResourceDictionaryPath(theme);
                    if (!string.IsNullOrEmpty(resourceDictionaryPath))
                    {
                        ResourceDictionary skinDictionary = new ResourceDictionary
                        {
                            Source = new Uri(resourceDictionaryPath, UriKind.RelativeOrAbsolute)
                        };
                        Resources.MergedDictionaries.Add(skinDictionary);
                    }
                }
            }
        }

        private void SetRegistryDefault(RegistryKey key, string name, string defaultValue)
        {
            if (key.GetValue(name) == null)
            {
                key.SetValue(name, defaultValue, RegistryValueKind.String);
            }
        }


        private void SetLanguageDictionary()
        {
            ResourceDictionary dict = new ResourceDictionary();
            switch (Thread.CurrentThread.CurrentCulture.ToString())
            {
                case "en-US":
                    dict.Source = new Uri("..\\MultiLang\\StringResources.xaml", UriKind.Relative);
                    break;
                case "pl-PL":
                    dict.Source = new Uri("..\\MultiLang\\StringResources.pl-PL.xaml", UriKind.Relative);
                    break;
                default:
                    dict.Source = new Uri("..\\MultiLang\\StringResources.xaml", UriKind.Relative);
                    break;
            }
            this.Resources.MergedDictionaries.Add(dict);
        }
        private string GetResourceDictionaryPath(string themeName)
        {
            // Define your mapping of theme names to ResourceDictionary paths
            // Example: "Fluent" theme corresponds to "Skins/Fluent.xaml"
            switch (themeName.ToLower())
            {
                case "fluent":
                    return "Skins/Fluent.xaml";
                case "metro":
                    return "Skins/Metro.xaml";
                case "classic":
                    return "Skins/Classic.xaml";
                case "vista":
                    return "Skins/Vista.xaml";
                case "seven":
                    return "Skins/Seven.xaml";
                case "hillel":
                    return "Skins/Aero.xaml";
                case "8cp":
                    return "Skins/8CP.xaml";
                case "8rp":
                    return "Skins/8RP.xaml";
                default:
                    return "Skins/Metro.xaml"; // Return default for unknown themes
            }
        }
    }
}

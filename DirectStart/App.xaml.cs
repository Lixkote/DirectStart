using System;
using System.Threading;
using System.IO;
using System.Windows;
using B8TAM;
using System.Diagnostics;

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

        protected override void OnStartup(StartupEventArgs e)
        {
            base.OnStartup(e);

            // Read the text file from %HOMEPATH%
            string filePath = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.UserProfile), "DirectStart", "Tiles", "config.txt");
            try
            {
                if (File.Exists(filePath))
                {
                    // Read the content of the text file
                    string[] lines = File.ReadAllLines(filePath);

                    // Initialize variables to hold configuration values
                    string theme = null;
                    string profilePictureShape = null;
                    string forceFillStartButton = null;
                    string retrobarfix = null;

                    // Parse each line of the config file
                    foreach (string line in lines)
                    {
                        string[] parts = line.Split('=');
                        if (parts.Length == 2)
                        {
                            string key = parts[0].Trim();
                            string value = parts[1].Trim();

                            // Apply configuration based on the key
                            switch (key.ToLower())
                            {
                                case "theme":
                                    theme = value;
                                    break;
                                case "profilepictureshape":
                                    profilePictureShape = value;
                                    break;
                                case "forcefillstartbutton":
                                    forceFillStartButton = value;
                                    break;
                                case "RetroBarFix":
                                    retrobarfix = value;
                                    break;
                                default:
                                    // Handle unknown keys if necessary
                                    break;
                            }
                        }
                    }

                    // Apply the theme
                    if (!string.IsNullOrEmpty(theme))
                    {
                        string resourceDictionaryPath = GetResourceDictionaryPath(theme);
                        if (!string.IsNullOrEmpty(resourceDictionaryPath))
                        {
                            // Set the ResourceDictionary for the theme
                            ResourceDictionary skinDictionary = new ResourceDictionary();
                            skinDictionary.Source = new Uri(resourceDictionaryPath, UriKind.RelativeOrAbsolute);
                            Resources.MergedDictionaries.Add(skinDictionary);
                        }
                        else
                        {
                            MessageBox.Show("Invalid theme specified in the config file.");
                        }
                    }

                    // Store profilePictureShape and forceFillStartButton in application-level resources
                    this.Resources["ProfilePictureShape"] = profilePictureShape;
                    this.Resources["ForceFillStartButton"] = forceFillStartButton;
                    this.Resources["RetroBarFix"] = retrobarfix;
                }
            }            
            catch (Exception ex)
            {
                MessageBox.Show(ex.ToString(), "B8taMenu had an issue loading the config file or its values.");
                Debug.WriteLine(ex.ToString(), "B8taMenu had an issue loading the config file or its values.");
            }

            // Initialize your main window or any other startup logic
            StartMenu mainWindow = new StartMenu();
            mainWindow.Show();
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

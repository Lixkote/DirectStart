using B8TAM.FrequentHelpers;
using Microsoft.Win32;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Data.OleDb;
using System.Diagnostics;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.IO;
using System.Linq;
using System.Management;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Forms;
using System.Windows.Input;
using System.Windows.Media;
using BlurryControls.Controls;
using System.Windows.Media.Animation;
using System.Xml;
using System.Xml.Serialization;
using Color = System.Windows.Media.Color;
using AFSM;
using System.IO.Pipes;
using System.Windows.Threading;

namespace B8TAM
{
	/// <summary>
	/// Interaction logic for StartMenu.xaml
	/// </summary>
	public partial class StartMenu : Window
	{
		StartMenuListener _listener;
		FrequentAppsHelper frequentHelper;
		ObservableCollection<StartMenuEntry> Programs = new ObservableCollection<StartMenuEntry>();
		ObservableCollection<StartMenuEntry> Pinned = new ObservableCollection<StartMenuEntry>();
		ObservableCollection<StartMenuEntry> Recent = new ObservableCollection<StartMenuEntry>();
		ObservableCollection<StartMenuLink> Results = new ObservableCollection<StartMenuLink>();
		ObservableCollection<Tile> Tiles = new ObservableCollection<Tile>();
		public SourceType SelectedSourceType { get; set; }
		private List<SourceType> sourceTypes;
		private ObservableCollection<CountEntry> countEntries;
		string forceFillStartButton;
        string isretrobarfix;
        string profilePictureShape;
		double taskbarheightinpx;
        string noTiles;

        public SolidColorBrush PressedBackground
        {
            get
            {
                IntPtr pElementName = Marshal.StringToHGlobalUni(ImmersiveColors.ImmersiveStartSelectionBackground.ToString());
                System.Windows.Media.Color color = GetColor(pElementName);
                return new SolidColorBrush(color);
            }
        }


        public ICommand Run => new RunCommand(RunCommand);

        private void StartPipeServer()
        {
            Task.Run(() =>
            {
                while (true)
                {
                    using (var server = new NamedPipeServerStream("DirectStartPipe", PipeDirection.In, NamedPipeServerStream.MaxAllowedServerInstances))
                    {
                        try
                        {
                            server.WaitForConnection();
                            using (var reader = new StreamReader(server))
                            {
                                var message = reader.ReadLine();
                                if (message != null && message.Contains("TRIGGER"))
                                {
                                    Dispatcher.Invoke(() => OnStartTriggeredNoArgs());
                                }
                                else
                                {
                                    Console.WriteLine("Received unknown message: " + message);
                                }
                            }
                        }
                        catch (IOException ex)
                        {
                            // Log or handle the error as needed
                            Console.WriteLine("Named pipe error: " + ex.Message);
                        }
                        catch (Exception ex)
                        {
                            // Catch other potential exceptions
                            Console.WriteLine("Error: " + ex.Message);
                        }
                    }
                }
            });
        }


        private static Color GetColor(IntPtr pElementName)
		{
			var colourset = DUIColorHelper.GetImmersiveUserColorSetPreference(false, false);
			uint type = DUIColorHelper.GetImmersiveColorTypeFromName(pElementName);
			Marshal.FreeCoTaskMem(pElementName);
			uint colourdword = DUIColorHelper.GetImmersiveColorFromColorSetEx((uint)colourset, type, false, 0);
			byte[] colourbytes = new byte[4];
			colourbytes[0] = (byte)((0xFF000000 & colourdword) >> 24); // A
			colourbytes[1] = (byte)((0x00FF0000 & colourdword) >> 16); // B
			colourbytes[2] = (byte)((0x0000FF00 & colourdword) >> 8); // G
			colourbytes[3] = (byte)(0x000000FF & colourdword); // R
			Color color = Color.FromArgb(colourbytes[0], colourbytes[3], colourbytes[2], colourbytes[1]);
			return color;
		}

		public StartMenu()
		{
			PreparePinnedStartMenu();
			string programs = System.IO.Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.CommonStartMenu), "Programs");
			GetPrograms(programs);
            programs = System.IO.Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.StartMenu), "Programs");
			GetPrograms(programs);
            StartPipeServer();

            string metroAppsDirectory = @"C:\Program Files\WindowsApps"; // Modify this as needed
            LoadMetroApps(metroAppsDirectory);

            string immersiveSet = @"C:\Windows\ImmersiveControlPanel"; // Modify this as needed
            LoadMetroApps(immersiveSet);


            this.sourceTypes = new List<SourceType>()
			{
				new SourceType("Program", @"Software\Microsoft\Windows\CurrentVersion\Explorer\UserAssist\{CEBFF5CD-ACE2-4F4F-9178-9926F41749EA}\Count"),
				new SourceType("Shortcut", @"Software\Microsoft\Windows\CurrentVersion\Explorer\UserAssist\{F4E57C4B-2036-45F0-A9AB-443BCFE33D9F}\Count")
			};
			this.SelectedSourceType = sourceTypes[0];
			GetFrequentsNew();

			string pinned = Path.Combine(
				Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData),
				@"Microsoft\Internet Explorer\Quick Launch\User Pinned\StartMenu\");
			GetPinned(pinned);

			Programs = new ObservableCollection<StartMenuEntry>(Programs.OrderBy(x => x.Title));
			Pinned = new ObservableCollection<StartMenuEntry>(Pinned.OrderBy(x => x.Title));

			InitializeComponent();

			UserImageButton.Tag = Environment.GetFolderPath(Environment.SpecialFolder.UserProfile);
			userpfp.Source = IconHelper.GetUserTile(Environment.UserName).ToBitmapImage();
			PinnedItems.ItemsSource = Pinned;
			ProgramsList.ItemsSource = Programs;
			RecentApps.ItemsSource = Recent;

            // CollectionView startListView = (CollectionView)CollectionViewSource.GetDefaultView(ProgramsList.ItemsSource);
            PropertyGroupDescription startGroupDesc = new PropertyGroupDescription("Alph");
			// startListView.GroupDescriptions.Add(startGroupDesc);
			_listener = new StartMenuListener();
			_listener.StartTriggered += OnStartTriggered;
			SearchGlyph.Source = Properties.Resources.SearchBoxGlyph.ToBitmapImage();
			// PowerGlyph.Source = Properties.Resources.powerglyph.ToBitmapImage();
			UserNameText.Text = Environment.UserName;

			DUIColorize();
            LoadTiles();
			AdjustToTaskbar();
            profilePictureShape = (string)System.Windows.Application.Current.Resources["ProfilePictureShape"];
            forceFillStartButton = (string)System.Windows.Application.Current.Resources["ForceFillStartButton"];
            noTiles = (string)System.Windows.Application.Current.Resources["NoTilesBool"];
            if (profilePictureShape == "rounded")
			{
				UserRounderer.CornerRadius = new CornerRadius(999);
            }
			else
			{
                UserRounderer.CornerRadius = new CornerRadius(0);
            }
            // Menu.Width = 273;
            // StartMenuBackground.Width = 273;
            if (noTiles == "true")
            {
				Debug.WriteLine("działa");
                Menu.Width = 267;
                StartMenuBackground.Width = 267;
            }
        }

		private void DUIColorize()
		{
            if (IsSkinSupportDuiBackgroundColor.Text == "True" || IsSkinSupportDuiBackgroundColor.Text == "true")
            {
                // DUI Colors for the main start menu grid:
                IntPtr pElementName = Marshal.StringToHGlobalUni(ImmersiveColors.ImmersiveStartBackground.ToString());
                System.Windows.Media.Color color = GetColor(pElementName);
                StartMenuBackground.Background = new SolidColorBrush(color);
                StartLogoTop.Background = new SolidColorBrush(color);
                StartLogoLeft.Background = new SolidColorBrush(color);
                StartLogoBottom.Background = new SolidColorBrush(color);
                StartLogoRight.Background = new SolidColorBrush(color);
            }
        }

        private double GetTaskbarHeight()
        {
            // First, try to get taskbar height using Screen class
            double taskbarHeight = GetTaskbarHeightUsingScreenClass();

            // If the taskbar height obtained is non-negative, return it
            if (taskbarHeight >= 0)
                return taskbarHeight;

            // If the taskbar height obtained is negative, try an alternate method
            taskbarHeight = GetTaskbarHeightUsingShell32();

            return taskbarHeight;
        }

        private double GetTaskbarHeightUsingScreenClass()
        {
            try
            {
                // Get the working area of the screen (excluding the taskbar)
                Rectangle workingArea = Screen.PrimaryScreen.WorkingArea;

                // Get the total area of the screen
                Rectangle screenArea = Screen.PrimaryScreen.Bounds;

                // Calculate the taskbar height by subtracting the working area height from the total screen height
                double taskbarHeight = screenArea.Height - workingArea.Height;

                return taskbarHeight;
            }
            catch
            {
                // Handle any exceptions gracefully
                return -1;
            }
        }

        private double GetTaskbarHeightUsingShell32()
        {
            try
            {
                APPBARDATA appBarData = new APPBARDATA();
                appBarData.cbSize = (uint)Marshal.SizeOf(appBarData);
                IntPtr result = SHAppBarMessage(ABM_GETTASKBARPOS, ref appBarData);
                if (result == IntPtr.Zero)
                    return -1;

                RECT taskbarRect = appBarData.rc;
                return taskbarRect.Bottom - taskbarRect.Top;
            }
            catch
            {
                // Handle any exceptions gracefully
                return -1;
            }
        }

        // P/Invoke declarations
        [StructLayout(LayoutKind.Sequential)]
        private struct APPBARDATA
        {
            public uint cbSize;
            public IntPtr hWnd;
            public uint uCallbackMessage;
            public uint uEdge;
            public RECT rc;
            public IntPtr lParam;
        }

        [StructLayout(LayoutKind.Sequential)]
        private struct RECT
        {
            public int Left;
            public int Top;
            public int Right;
            public int Bottom;
        }

        [DllImport("shell32.dll")]
        private static extern IntPtr SHAppBarMessage(uint dwMessage, ref APPBARDATA pData);

        private const uint ABM_GETTASKBARPOS = 5;

        private double GetTaskbarWidth()
        {
            // First, try to get taskbar width using Screen class
            double taskbarWidth = GetTaskbarWidthUsingScreenClass();

            // If the taskbar width obtained is non-negative, return it
            if (taskbarWidth >= 0)
                return taskbarWidth;

            // If the taskbar width obtained is negative, try an alternate method
            taskbarWidth = GetTaskbarWidthUsingShell32();

            return taskbarWidth;
        }

        private double GetTaskbarWidthUsingScreenClass()
        {
            try
            {
                // Get the working area of the screen (excluding the taskbar)
                Rectangle workingArea = Screen.PrimaryScreen.WorkingArea;

                // Get the total area of the screen
                Rectangle screenArea = Screen.PrimaryScreen.Bounds;

                // Calculate the taskbar width by subtracting the working area width from the total screen width
                double taskbarWidth = screenArea.Width - workingArea.Width;

                return taskbarWidth;
            }
            catch
            {
                // Handle any exceptions gracefully
                return -1;
            }
        }

        private double GetTaskbarWidthUsingShell32()
        {
            try
            {
                APPBARDATA appBarData = new APPBARDATA();
                appBarData.cbSize = (uint)Marshal.SizeOf(appBarData);
                IntPtr result = SHAppBarMessage(ABM_GETTASKBARPOS, ref appBarData);
                if (result == IntPtr.Zero)
                    return -1;

                RECT taskbarRect = appBarData.rc;
                return taskbarRect.Right - taskbarRect.Left;
            }
            catch
            {
                // Handle any exceptions gracefully
                return -1;
            }
        }

        private void AdjustToTaskbar()
        {
			var desktopWorkingArea = SystemParameters.WorkArea;
			// Get the screen
			Screen screen = Screen.FromHandle(new System.Windows.Interop.WindowInteropHelper(this).Handle);
            // Get the taskbar height
            taskbarheightinpx = GetTaskbarHeight();
            double taskbarwidthinpx = GetTaskbarWidth();
			var taskbarPosition = GetTaskbarPosition.Taskbar.Position;
            Version osVersion = Environment.OSVersion.Version;

            switch (taskbarPosition)
			{
				case GetTaskbarPosition.TaskbarPosition.Top:
					StartMenuBackground.VerticalAlignment = VerticalAlignment.Bottom;
					double ht = Menu.Height + taskbarheightinpx;
					Menu.Height = ht;
					base.Left = 0.0;
					base.Top = 0.0;
                    if (osVersion.Major == 6 && osVersion.Minor == 3)
                    {
						// Windows 8.1
                        StartLogoTop.Visibility = Visibility.Visible;
                    }
                    else if (forceFillStartButton == "true")
                    {
						// Windows 8
                        StartLogoTop.Visibility = Visibility.Visible;
                    }
                    else
                    {
                        StartLogoTop.Visibility = Visibility.Hidden;
                    }
                    StartLogoTop.Height = taskbarheightinpx + 1;
					Menu.Margin = new Thickness(0, taskbarheightinpx, 0, 0);
                    if (forceFillStartButton == "true")
                    {
                        // Windows 8
                        StartLogoTop.Visibility = Visibility.Visible;
                    }
                    break;
				case GetTaskbarPosition.TaskbarPosition.Bottom:
					// Taskbar on Bottom
					StartMenuBackground.VerticalAlignment = VerticalAlignment.Top;
					double hb = Menu.Height + taskbarheightinpx;
					Menu.Height = hb;
					base.Left = 0.0;
					base.Top = SystemParameters.WorkArea.Bottom - base.Height + taskbarheightinpx;
                    if (osVersion.Major == 6 && osVersion.Minor == 3)
                    {
                        // Windows 8.1
                        StartLogoBottom.Visibility = Visibility.Visible;
                    }
                    else if (forceFillStartButton == "true")
                    {
                        // Windows 8
                        StartLogoBottom.Visibility = Visibility.Visible;
                    }
					else
					{
                        StartLogoBottom.Visibility = Visibility.Hidden;
                    }
					try
					{
                        StartLogoBottom.Height = taskbarheightinpx + 1;
                        Menu.Margin = new Thickness(0, 0, 0, taskbarheightinpx);
                        if (forceFillStartButton == "true")
                        {
                            // Windows 8
                            StartLogoBottom.Visibility = Visibility.Visible;
                        }
                    }
					catch (Exception ex)
					{
                        StartLogoBottom.Height = 48 + 1;
                        Menu.Margin = new Thickness(0, 0, 0, 48);
                        if (forceFillStartButton == "true")
                        {
                            // Windows 8
                            StartLogoBottom.Visibility = Visibility.Visible;
                        }
                        Debug.WriteLine("Taskbar height exception: " + ex.ToString());
					}
                    break;
				case GetTaskbarPosition.TaskbarPosition.Left:
					// Taskbar on left
					StartMenuBackground.HorizontalAlignment = System.Windows.HorizontalAlignment.Left;
					double hl = Menu.Width + taskbarwidthinpx;
					Menu.Width = hl;
					Menu.Left = 0.0;
					base.Top = 0.0;
					StartLogoLeft.Width = taskbarwidthinpx;
                    if (osVersion.Major == 6 && osVersion.Minor == 3)
                    {
                        // Windows 8.1
                        StartLogoLeft.Visibility = Visibility.Visible;
                    }
                    else if (forceFillStartButton == "true")
                    {
                        // Windows 8
                        StartLogoLeft.Visibility = Visibility.Visible;
                    }
                    else
                    {
                        StartLogoLeft.Visibility = Visibility.Hidden;
                    }
					break;
				case GetTaskbarPosition.TaskbarPosition.Right:
					// Taskbar on right
					StartMenuBackground.HorizontalAlignment = System.Windows.HorizontalAlignment.Left;
					double hr = Menu.Width + taskbarwidthinpx;
					Menu.Width = hr;
					base.Top = 0.0;
					StartLogoRight.Width = taskbarwidthinpx;
                    if (osVersion.Major == 6 && osVersion.Minor == 3)
                    {
                        // Windows 8.1
                        StartLogoRight.Visibility = Visibility.Visible;
                    }
                    else if (forceFillStartButton == "true")
                    {
                        // Windows 8
                        StartLogoRight.Visibility = Visibility.Visible;
                    }
                    else
                    {
                        StartLogoRight.Visibility = Visibility.Hidden;
                    }
                    break;
				case GetTaskbarPosition.TaskbarPosition.Unknown:
					// Default case where we cannot detect taskbar position
					StartMenuBackground.VerticalAlignment = VerticalAlignment.Top;
					double hu = Menu.Height + taskbarheightinpx;
					Menu.Height = hu;
					base.Left = 0.0;
					base.Top = SystemParameters.WorkArea.Bottom - base.Height + taskbarheightinpx;
                    if (osVersion.Major == 6 && osVersion.Minor == 3)
                    {
                        // Windows 8.1
                        StartLogoBottom.Visibility = Visibility.Visible;
                    }
                    else if (forceFillStartButton == "true")
                    {
                        // Windows 8
                        StartLogoBottom.Visibility = Visibility.Visible;
                    }
                    else
                    {
                        StartLogoBottom.Visibility = Visibility.Hidden;
                    }
                    StartLogoBottom.Height = taskbarheightinpx + 1;
					Menu.Margin = new Thickness(0, 0, 0, taskbarheightinpx);
					break;
				default:
					break;
			}
		}

		int maxfrequent = 5;
		int startfrequent = 0;

        private string GetDisplayNameFromExePath(string exePath)
        {
            string displayName = string.Empty;

			try
			{
				var fileVersionInfo = System.Diagnostics.FileVersionInfo.GetVersionInfo(exePath);
				displayName = fileVersionInfo.FileDescription;

				// If display name is longer than 17 characters, get the path without extension
				if (displayName != null)
				{
                    if (displayName.Length > 25)
                    {
                        displayName = Path.GetFileNameWithoutExtension(exePath);
                    }
                }
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"An error occurred: {ex.Message}", "Error");
            }

            return displayName;
        }

        private void GetFrequentsNew()
		{
			if (startfrequent <= maxfrequent)
			{
				try
				{
					RegistryKey reg = Registry.CurrentUser.OpenSubKey(SelectedSourceType.Key);
					List<CountEntry> sortedEntries = new List<CountEntry>();

					foreach (string valueName in reg.GetValueNames())
					{
						CountEntry entry = new CountEntry();
						entry.Name = valueName;
						entry.Value = (byte[])reg.GetValue(valueName);
						entry.RegKey = reg.ToString();

						if (File.Exists(entry.DecodedName) || Directory.Exists(entry.DecodedName))
						{
							sortedEntries.Add(entry);
						}
					}

					// Sort the entries based on executionCount in descending order
					sortedEntries.Sort((a, b) => b.ExecutionCount.CompareTo(a.ExecutionCount));

					// Add sorted entries to Recent
					foreach (CountEntry entry in sortedEntries)
					{
                        if(!entry.DecodedName.Contains("Start menu"))
						{
							Recent.Add(new StartMenuLink
							{
								Title = GetDisplayNameFromExePath(entry.DecodedName),
								Icon = IconHelper.GetFileIcon(entry.DecodedName),
								Link = entry.DecodedName
							});
						}

						startfrequent++;

						if (startfrequent >= maxfrequent)
						{
							break; // Break if maxfrequent limit reached
						}
					}
				}
				catch (Exception ex)
				{
					System.Windows.MessageBox.Show("Error reading UserAssist entries: " + ex.Message);
				}
			}
		}

		private void UIElement_OnMouseLeftButtonUp(object sender, MouseButtonEventArgs e)
		{
			e.Handled = false;
		}


		private void Item_MouseEnter(object sender, System.Windows.Input.MouseEventArgs e)
		{
			if (sender is StackPanel panel)
			{
				// Change the background color or any other visual properties
				panel.Background = new System.Windows.Media.SolidColorBrush(System.Windows.Media.Color.FromArgb(80, 255, 255, 255));
			}
		}

		private void Item_MouseLeave(object sender, System.Windows.Input.MouseEventArgs e)
		{
			if (sender is StackPanel panel)
			{
				// Revert back to the original background color or visual properties
				panel.Background = System.Windows.Media.Brushes.Transparent; // or any other color you desire
			}
		}


		private void PreparePinnedStartMenu() 
		{
			// Specify the directory path
			string targetDirectory = Path.Combine(
				Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData),
				@"Microsoft\Internet Explorer\Quick Launch\User Pinned\");

			// Specify the folder name to check
			string folderToCheck = "StartMenu";

			// Check if the folder exists
			if (!Directory.Exists(Path.Combine(targetDirectory, folderToCheck)))
			{
				// If not, create the folder
				try
				{
					Directory.CreateDirectory(Path.Combine(targetDirectory, folderToCheck));
				}
				catch (Exception ex)
				{
					Debug.WriteLine("Error Creatin Folder :C" + ex.ToString());
				}
			}
			else
			{
			}
		}

		public void LoadTiles()
		{
			TilesLoader TileAppHelper = new TilesLoader();
			Tiles = new ObservableCollection<Tile>();
			TileAppHelper.LoadTileGroups(Tiles);
			TilesHost.ItemsSource = Tiles;
		}

        void OnStartTriggered(object sender, EventArgs e)
        {
            ToggleStartMenu();
        }

        void OnStartTriggeredNoArgs()
        {
            ToggleStartMenu();
        }

        public async void ToggleStartMenu()
        {
            try
            {
                if (Visibility == Visibility.Visible)
                {
                    Hide();
                }
                else
                {
                    Show();
                    // AdjustToTaskbar();
                    DUIColorize();
                    WindowActivator.ActivateWindow(new System.Windows.Interop.WindowInteropHelper(Menu).Handle);
                    SearchText.Focus();
                }
            }
            catch (Exception ex)
            {
				Debug.WriteLine(ex.ToString());
            }
        }


        private void Window_Activated(object sender, EventArgs e)
		{
			Screen screen = Screen.FromPoint(System.Windows.Forms.Control.MousePosition);
			this.Left = screen.WorkingArea.Left;
            if (IsDwmBlurEnabled.Text == "False" || IsDwmBlurEnabled.Text == "false")
			{
				// Do not enable blur
			}
			else if (IsDwmBlurEnabled.Text == "True" || IsDwmBlurEnabled.Text == "true")
			{
				// Enable blur
				BlurEffect.EnableBlur(this);
            }
		}

        private void HandleCheck(object sender, RoutedEventArgs e)
		{
			GridPrograms.Visibility = Visibility.Visible;
			GridTogglable.Visibility = Visibility.Collapsed;
            ToggleButtonText.Visibility = Visibility.Collapsed;
            ToggleButtonTextBack.Visibility = Visibility.Visible;
            ToggleButtonGlyph.Text = "";
			ToggleButtonGlyph.FontFamily = new System.Windows.Media.FontFamily("Segoe UI Symbol");
        }

		private void HandleUnchecked(object sender, RoutedEventArgs e)
		{
			GridPrograms.Visibility = Visibility.Collapsed;
			GridTogglable.Visibility = Visibility.Visible;
            ToggleButtonText.Visibility = Visibility.Visible;
            ToggleButtonTextBack.Visibility = Visibility.Collapsed;
            ToggleButtonGlyph.Text = "";
            ToggleButtonGlyph.FontFamily = new System.Windows.Media.FontFamily("Segoe UI Symbol");
        }

		private void Menu_Deactivated(object sender, EventArgs e)
		{
			Results.Clear();
			SearchText.Text = string.Empty;
			Visibility = Visibility.Hidden;
			Hide();
		}

        // Method to load Metro apps into the Programs list
        private void LoadMetroApps(string metroAppsDirectory)
        {
            // Assuming GetMetroApps is a static method and takes the directory where metro apps are stored
            var metroApps = MetroAppHelper.GetMetroApps(metroAppsDirectory);

            // Add each metro app to the ObservableCollection
            foreach (var metroApp in metroApps)
            {
                Programs.Add(new MetroApp
                {
                    Name = metroApp.Name,
					Path = metroApp.Path,
                    Icon = metroApp.Icon,  // You can display an icon, or handle it as needed
                    Identity = metroApp.Identity
                });
            }
        }

        private void GetPrograms(string directory)
		{
			foreach (string f in Directory.GetFiles(directory))
			{
				if (System.IO.Path.GetExtension(f) != ".ini")
				{
					Programs.Add(new StartMenuLink
					{
						Title = System.IO.Path.GetFileNameWithoutExtension(f),
						Icon = IconHelper.GetFileIcon(f),
						Link = f
					});
				}
			}
			GetProgramsRecurse(directory);
		}

		private void GetPinned(string directory)
		{
			foreach (string f in Directory.GetFiles(directory))
			{
				if (System.IO.Path.GetExtension(f) != ".ini")
				{
					Pinned.Add(new StartMenuLink
					{
						Title = System.IO.Path.GetFileNameWithoutExtension(f),
						Icon = IconHelper.GetFileIcon(f),
						Link = f
					});
				}
			}
			GetProgramsRecurse(directory);
		}
		private void GetProgramsRecurse(string directory, StartMenuDirectory parent = null)
		{
			bool hasParent = parent != null;
			foreach (string d in Directory.GetDirectories(directory))
			{
				StartMenuDirectory folderEntry = null;
				if (!hasParent)
				{
					folderEntry = Programs.FirstOrDefault(x => x.Title == new DirectoryInfo(d).Name) as StartMenuDirectory;
				}
				if (folderEntry == null)
				{
					folderEntry = new StartMenuDirectory
					{
						Title = new DirectoryInfo(d).Name,
						Links = new ObservableCollection<StartMenuLink>(),
						Directories = new ObservableCollection<StartMenuDirectory>(),
						Link = d,
						Icon = IconHelper.GetFolderIcon(d)
					};
				}
				
				GetProgramsRecurse(d, folderEntry);
				foreach (string f in Directory.GetFiles(d))
				{
					folderEntry.HasChildren = true;
					if (System.IO.Path.GetExtension(f) != ".ini")
					{
						folderEntry.Links.Add(new StartMenuLink
						{
							Title = System.IO.Path.GetFileNameWithoutExtension(f),
							Icon = IconHelper.GetFileIcon(f),
							Link = f
						});
					}
				}
				
				if (!hasParent)
				{
					if (!Programs.Contains(folderEntry))
					{
						Programs.Add(folderEntry);
					}
				}
				else
				{
					parent.Directories.Add(folderEntry);
				}
			}
		}
		
		private void Link_Click(object sender, RoutedEventArgs e)
		{
			Link_Click(sender, null);
		}

		private void Tile_Click(object sender, RoutedEventArgs e)
		{
			Tile_Click(sender, null);
		}
		private void Tile_Click(object sender, MouseButtonEventArgs e)
		{
			Tile data = (sender as FrameworkElement).DataContext as Tile;
			this.Hide();
			Process.Start(data.Path);
		}

		private void BrowseLink_Click(object sender, RoutedEventArgs e)
		{
			StartMenuLink data = (sender as FrameworkElement).DataContext as StartMenuLink;
			this.Hide();
			Process.Start("explorer.exe", $"/select, \"{data.Link}\"");
		}

		private void Link_Click(object sender, MouseButtonEventArgs e)
		{
			StartMenuLink data = (sender as FrameworkElement).DataContext as StartMenuLink;
			this.Hide();
			Process.Start(data.Link);
		}

		private void TextBox_TextChanged(object sender, TextChangedEventArgs e)
		{
			var searchTerm = SearchText.Text;
			Search(searchTerm);
		}
		Thread _searchThread = null;
		private void Search(string searchTerm)
		{
			if(_searchThread != null)
			{
				_searchThread.Abort();
			}
			Results.Clear();
			if (searchTerm.Length == 0)
			{
				return;
			}
			_searchThread = new Thread(() => { 
				var matches = Programs.Search(searchTerm);
				foreach(StartMenuLink link in matches)
				{
					Dispatcher.InvokeAsync(() => { 
						Results.Add(new StartMenuSearchResult
						{
							Icon = link.Icon,
							Link = link.Link,
							Title = link.Title,
							ResultType = ResultType.Apps
						});
					});
				}
				SearchDocuments(searchTerm);
			});
			_searchThread.Start();
		}

		private void Folder_Click(object sender, RoutedEventArgs e)
		{
			string folder = (sender as System.Windows.Controls.Control).Tag as String;
			if(folder == string.Empty)
			{
				folder = "explorer.exe";
			}
			Process.Start(folder);
		}

		private void SearchDocuments(string searchTerm)
		{
			using (var connection = new OleDbConnection(@"Provider=Search.CollatorDSO;Extended Properties=""Application=Windows"""))
			{
				var query = @"SELECT TOP 15 System.ItemName, System.ItemUrl, System.ItemType FROM SystemIndex " +
				$@"WHERE scope ='file:{Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments)}' {(searchTerm.Length < 3 ? "AND System.ItemType = 'Directory'" : "")} AND System.ItemName LIKE '%{searchTerm}%'";
				connection.Open();
				using (var command = new OleDbCommand(query, connection))
				using (var r = command.ExecuteReader())
				{
					while (r.Read())
					{
						string fileName = r[0] as string;
						string filePath = r[1] as string;
						string itemType = r[2] as string;
						filePath = filePath.Remove(0, 5).Replace("/", "\\");
						Dispatcher.InvokeAsync(() =>
						{
							Results.Add(new StartMenuSearchResult
							{
								Title = itemType == "Directory" ? fileName : System.IO.Path.GetFileName(fileName),
								Icon = IconHelper.GetFileIcon(filePath),//need cache for performance
								Link = filePath,
								AllowOpenLocation = itemType == "Directory" ? false : true,
								ResultType = ResultType.Files
							});
						});
					}
				}
			}
		}

		private void PowerButton_Click(object sender, RoutedEventArgs e)
		{
			PowerMenu.PlacementTarget = sender as UIElement;
			PowerMenu.Placement = System.Windows.Controls.Primitives.PlacementMode.Bottom;
			PowerMenu.IsOpen = true;
		}
		public void RunCommand(string command)
		{
			Task.Factory.StartNew(() => { 

				ProcessStartInfo processStartInfo;

				processStartInfo = new ProcessStartInfo(command.Split(' ')[0]);
				processStartInfo.Arguments = command.Remove(0, command.Split(' ')[0].Length);
				processStartInfo.CreateNoWindow = true;
				processStartInfo.UseShellExecute = true;

				try
				{
					Process.Start(processStartInfo);
				}
				catch(Exception ex)
				{
					Dispatcher.Invoke(() => { System.Windows.MessageBox.Show(ex.Message); });
				}
			});
		}

		private void Shutdown(object sender, RoutedEventArgs e)
		{
			System.Diagnostics.Process.Start("shutdown.exe", "-s -t 0");
		}

		private void Sleep(object sender, RoutedEventArgs e)
		{
			System.Windows.Forms.Application.SetSuspendState(PowerState.Suspend, true, true);
		}

		private void Restart(object sender, RoutedEventArgs e)
		{
			System.Diagnostics.Process.Start("shutdown.exe", "-r -t 0");
		}

		private void Hibernate(object sender, RoutedEventArgs e)
		{
			System.Windows.Forms.Application.SetSuspendState(PowerState.Hibernate, true, true);
		}

        private void MenuItem_Click(object sender, RoutedEventArgs e)
        {
        }

        private void UserImageButton_Click(object sender, RoutedEventArgs e)
        {
			UserMenu.PlacementTarget = sender as UIElement;
			UserMenu.Placement = System.Windows.Controls.Primitives.PlacementMode.Bottom;
			UserMenu.IsOpen = true;
		}

        private void sleepPC_Click(object sender, RoutedEventArgs e)
        {
			System.Windows.Forms.Application.SetSuspendState(PowerState.Suspend, true, true);
		}

		private void shutdownPC_Click(object sender, RoutedEventArgs e)
        {
			System.Diagnostics.Process.Start("shutdown.exe", "-s -t 0");
		}

		private void restartPC_Click(object sender, RoutedEventArgs e)
        {
			System.Diagnostics.Process.Start("shutdown.exe", "-r -t 0");
		}

		private void ExitDS_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                // Get all processes named "Start menu.exe"
                var processes = Process.GetProcessesByName("Start menu");

                if (processes.Length == 0)
                {
                    return;
                }

                // Kill each found process
                foreach (var process in processes)
                {
                    process.Kill();
                    process.WaitForExit();
                }
            }
            catch (Exception ex)
            {

            }
        }

        private void Menu_Loaded(object sender, RoutedEventArgs e)
        {
			GridPrograms.Visibility = Visibility.Collapsed;
		}

        private void StartLogo_Click(object sender, RoutedEventArgs e)
        {
			OnStartTriggered(sender, e);
        }

        private void ResizeTileSmall_Click(object sender, RoutedEventArgs e)
        {
			var h = sender as System.Windows.Controls.MenuItem;
			var b = h.DataContext as Tile;
			b.Size = "Small";
			TilesHost.ItemsSource = null;
            LoadTiles();
            TilesHost.ItemsSource = Tiles;
        }

        private void ResizeTileNormal_Click(object sender, RoutedEventArgs e)
        {

        }

        private void ResizeTileWide_Click(object sender, RoutedEventArgs e)
        {

        }

        private void ResizeTileLarge_Click(object sender, RoutedEventArgs e)
        {

        }

        private void SearchText_PreviewKeyDown(object sender, System.Windows.Input.KeyEventArgs e)
        {
            // Check if any alphanumeric key is pressed
            if (char.IsLetterOrDigit((char)e.Key))
            {
                string rundll32Path = Environment.ExpandEnvironmentVariables(@"%windir%\system32\rundll32.exe");
                string command = @"-sta {C90FB8CA-3295-4462-A721-2935E83694BA}";

                ProcessStartInfo startInfo = new ProcessStartInfo(rundll32Path, command)
                {
                    CreateNoWindow = true,
                    UseShellExecute = false
                };

                try
                {
                    Process.Start(startInfo);
                }
                catch (Exception ex)
                {
                    Debug.WriteLine("An error occurred: " + ex.Message);
                }
            }
        }

        private void SearchText_TextChanged(object sender, TextChangedEventArgs e)
        {
            string rundll32Path = Environment.ExpandEnvironmentVariables(@"%windir%\system32\rundll32.exe");
            string command = @"-sta {C90FB8CA-3295-4462-A721-2935E83694BA}";

            ProcessStartInfo startInfo = new ProcessStartInfo(rundll32Path, command)
            {
                CreateNoWindow = true,
                UseShellExecute = false
            };

            try
            {
                Process.Start(startInfo);
            }
            catch (Exception ex)
            {
                Debug.WriteLine("An error occurred: " + ex.Message);
            }
        }

        private void UserImageChangeMenuItem_Click(object sender, RoutedEventArgs e)
        {

        }

        private void LockMenuItem_Click(object sender, RoutedEventArgs e)
        {

        }

        private void SignOutMenuItem_Click(object sender, RoutedEventArgs e)
        {

        }
    }

    public class RunCommand : ICommand
	{
		public delegate void ExecuteMethod();
		private Action<string> func;
		public RunCommand(Action<string> exec)
		{
			func = exec;
		}

		public bool CanExecute(object parameter)
		{
			return true;
		}

		public event EventHandler CanExecuteChanged;

		public void Execute(object parameter)
		{
			func(parameter as string);
		}
	}
}

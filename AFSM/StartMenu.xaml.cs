using System;
using System.Collections.ObjectModel;
using System.Data.OleDb;
using System.Diagnostics;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Forms;
using System.Windows.Input;
using System.Windows.Media.Animation;
using System.Xml;
using System.Xml.Serialization;


namespace AFSM
{
	/// <summary>
	/// Interaction logic for StartMenu.xaml
	/// </summary>
	public partial class StartMenu : Window
	{
		StartMenuListener _listener;
		ObservableCollection<StartMenuEntry> Programs = new ObservableCollection<StartMenuEntry>();
		ObservableCollection<StartMenuEntry> Pinned = new ObservableCollection<StartMenuEntry>();
		ObservableCollection<StartMenuLink> Results = new ObservableCollection<StartMenuLink>();

		public ICommand Run => new RunCommand(RunCommand);

		public StartMenu()
		{
			PreparePinnedStartMenu();
			string programs = System.IO.Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.CommonStartMenu), "Programs");
			GetPrograms(programs);
			programs = System.IO.Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.StartMenu), "Programs");
			GetPrograms(programs);

			string pinned = Path.Combine(
				Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData),
				@"Microsoft\Internet Explorer\Quick Launch\User Pinned\StartMenu\");
			GetPinned(pinned);

			Programs = new ObservableCollection<StartMenuEntry>(Programs.OrderBy(x => x.Title));
			Pinned = new ObservableCollection<StartMenuEntry>(Pinned.OrderBy(x => x.Title));

			InitializeComponent();

			UserImageButton.Tag = Environment.GetFolderPath(Environment.SpecialFolder.UserProfile);
			UserImage.Source = IconHelper.GetUserTile(Environment.UserName).ToBitmapImage();
			PinnedItems.ItemsSource = Pinned;
			ProgramsList.ItemsSource = Programs;
			
			// CollectionView startListView = (CollectionView)CollectionViewSource.GetDefaultView(ProgramsList.ItemsSource);
			PropertyGroupDescription startGroupDesc = new PropertyGroupDescription("Alph");
			// startListView.GroupDescriptions.Add(startGroupDesc);
			
			var desktopWorkingArea = SystemParameters.WorkArea;
			Left = 0;
			Top = desktopWorkingArea.Bottom - Height;
			_listener = new StartMenuListener();
			_listener.StartTriggered += OnStartTriggered;
			SearchGlyph.Source = Properties.Resources.SearchBoxGlyph.ToBitmapImage();
			PowerGlyph.Source = Properties.Resources.powerglyph.ToBitmapImage();
			UserNameText.Text = Environment.UserName;
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

		
		void OnStartTriggered(object sender, EventArgs e)
		{
			Visibility = Visibility == Visibility.Visible ? Visibility.Hidden : Visibility.Visible;
			if (Visibility == Visibility.Visible)
			{
				Show();
				WindowActivator.ActivateWindow(new System.Windows.Interop.WindowInteropHelper(Menu).Handle);
				SearchText.Focus();
			}
		}
		
		private void Window_Activated(object sender, EventArgs e)
		{
			Screen screen = Screen.FromPoint(System.Windows.Forms.Control.MousePosition);
			this.Left = screen.WorkingArea.Left;
			BlurEffect.EnableBlur(this);
		}

		private void HandleCheck(object sender, RoutedEventArgs e)
		{
			GridPrograms.Visibility = Visibility.Visible;
			GridTogglable.Visibility = Visibility.Collapsed;
			ToggleButtonText.Text = "Back";
			ToggleButtonGlyph.Text = "";
		}

		private void HandleUnchecked(object sender, RoutedEventArgs e)
		{
			GridPrograms.Visibility = Visibility.Collapsed;
			GridTogglable.Visibility = Visibility.Visible;
			ToggleButtonText.Text = "All Apps";
			ToggleButtonGlyph.Text = "";
		}

		private void Menu_Deactivated(object sender, EventArgs e)
		{
			Results.Clear();
			SearchText.Text = string.Empty;
			Visibility = Visibility.Hidden;
			Hide();
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

using System;
using System.Collections.ObjectModel;
using System.Windows.Controls;
using System.Windows.Media;

namespace B8TAM
{
	public enum LinkType
	{
		File,
		Directory
	}
	internal abstract class StartMenuEntry
	{
		public string Alph { get { return Title[0].ToString(); } }
		public string Title { get; set; }
		public ImageSource Icon { get; set; }
	}
	internal class StartMenuDirectory : StartMenuLink
	{
		public bool HasChildren { get; set; }
		public ObservableCollection<StartMenuLink> Links { get; set; }
		public ObservableCollection<StartMenuDirectory> Directories { get; set; }
	}
	internal class StartMenuLink : StartMenuEntry
	{
		public string Link { get; set; }
		public bool AllowOpenLocation { get; set; }
	}

	public enum ResultType
	{
		Files,
		Apps
	}
	internal class StartMenuSearchResult : StartMenuLink
	{
		public ResultType ResultType { get; set; }
	}
}

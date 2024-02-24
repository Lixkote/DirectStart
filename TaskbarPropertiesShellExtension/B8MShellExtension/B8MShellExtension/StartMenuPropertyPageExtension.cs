using SharpShell.SharpPropertySheet;
using System;
using System.IO;
using System.Linq;
using System.Windows.Forms;


namespace B8MShellExtension
{
    public partial class StartMenuPropertyPageExtension : SharpPropertyPage
    {
        public StartMenuPropertyPageExtension()
        {
            InitializeComponent();

            //  Set the page title.
            PageTitle = "Start menu";
        }

        protected override void OnPropertyPageInitialised(SharpPropertySheet parent)
        {
        }
    }
}

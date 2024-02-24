using SharpShell.Attributes;
using SharpShell.SharpPropertySheet;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;

namespace B8MShellExtension
{
    [ComVisible(true)]
    [COMServerAssociation(AssociationType.ClassOfExtension, ".txt")]
    public class FileTimesPropertySheet : SharpPropertySheet
    {
        protected override bool CanShowSheet()
        {
            //  We will only show the resources pages if we have ONE file selected.
            // return SelectedItemPaths.Count() == 1;
            return true;
        }

        protected override IEnumerable<SharpPropertyPage> CreatePages()
        {
            //  Create the property sheet page.
            var page = new StartMenuPropertyPageExtension();

            //  Return the pages we've created.
            return new[] { page };
        }
    }
}

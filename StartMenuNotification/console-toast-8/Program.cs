using NDesk.Options;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Win8Toast;

namespace console_toast_8
{
    class Program
    {
        static void Main(string[] args)
        {
            string template = "", title = "", line1 = "", line2 = "", img = "", icon = "", app_id = "console-toast-8";
            bool silent = false;

            OptionSet p = new OptionSet()
                .Add("template=",   "toast template",                                   x => template = x)
                .Add("title=",      "toast tile",                                       x => title = EnsureStringUsesUTF8(x))
                .Add("line1=",      "line one text (all templates)",                    x => line1 = EnsureStringUsesUTF8(x))
                .Add("line2=",      "line two text (Text/Text&Image04 templates)",      x => line2 = EnsureStringUsesUTF8(x))
                .Add("img=",        "img url (local or external, all img templates)",   x => img = x)
                .Add("app_id=",     "name to appear in start menu (optional)",          x => app_id = x)
                .Add("icon=",       "path to .ico (optional)",                          x => icon = x)
                .Add("silent",      "prevent default notification noise (optional)",    x => silent = x != null);

            p.Parse(args);



            Toast.APP_ID = app_id;
            Toast.silent = silent;
            Toast.ICON_LOCATION = "C:\blank.ico";
            Toast.TryCreateShortcut();

            Toast.ToastToastText02("The Start menu is back!", "Click to find out more.");

            Environment.Exit(0);
        }

        static String EnsureStringUsesUTF8(String str)
        {
            if (str != null && str.Length > 0)
                return Encoding.UTF8.GetString(Encoding.Default.GetBytes(str));
            else
                return str;
        }
    }
}

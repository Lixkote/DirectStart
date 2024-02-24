using Microsoft.WindowsAPICodePack.Shell.PropertySystem;
using MS.WindowsAPICodePack.Internal;
using ShellHelpers;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Windows.Data.Xml.Dom;
using Windows.UI.Notifications;

namespace Win8Toast
{
    class Toast
    {
        //Mostly refactored from the nice folks at MS

        public static string APP_ID = "";
        public static string ICON_LOCATION =  "C:\blank.ico";
        public static bool silent = false;

        public static bool TryCreateShortcut()
        {
            String shortcutPath = System.IO.Path.Combine(
                Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData),
                String.Format("Microsoft\\Windows\\Start Menu\\Programs\\{0}.lnk", APP_ID)
            );

            if (File.Exists(shortcutPath))
                // File.Delete(shortcutPath);

            InstallShortcut(shortcutPath);

            return true;
        }

        public static void InstallShortcut(String shortcutPath)
        {
            // Find the path to the current executable
            String exePath = Process.GetCurrentProcess().MainModule.FileName;
            IShellLinkW newShortcut = (IShellLinkW)new CShellLink();

            // Create a shortcut to the exe
            ShellHelpers.ErrorHelper.VerifySucceeded(newShortcut.SetPath(exePath));
            ShellHelpers.ErrorHelper.VerifySucceeded(newShortcut.SetArguments(""));

            //Add icon to shortcut
            if ("blank.ico" != string.Empty && File.Exists("blank.ico"))
                ShellHelpers.ErrorHelper.VerifySucceeded(newShortcut.SetIconLocation("blank.ico", 0));

            // Open the shortcut property store, set the AppUserModelId property
            IPropertyStore newShortcutProperties = (IPropertyStore)newShortcut;

            using (PropVariant appId = new PropVariant(APP_ID))
            {
                ShellHelpers.ErrorHelper.VerifySucceeded(newShortcutProperties.SetValue(
                    SystemProperties.System.AppUserModel.ID, appId));
                ShellHelpers.ErrorHelper.VerifySucceeded(newShortcutProperties.Commit());
            }

            // Commit the shortcut to disk
            ShellHelpers.IPersistFile newShortcutSave = (ShellHelpers.IPersistFile)newShortcut;

            ShellHelpers.ErrorHelper.VerifySucceeded(newShortcutSave.Save(shortcutPath, true));
        }

        // Create and show the toast.
        public static void ShowToast(XmlDocument xml)
        {
            // Create the toast and attach event listeners
            ToastNotification toast = new ToastNotification(xml);

            // Show the toast. Be sure to specify the AppUserModelId on your application's shortcut!
            ToastNotificationManager.CreateToastNotifier(APP_ID).Show(toast);
        }

        public static void ToastToastImageAndText01(string line1, string img)
        {
            // Get a toast XML template
            XmlDocument toastXml = ToastNotificationManager.GetTemplateContent(
                ToastTemplateType.ToastImageAndText01);

            // Fill in the text elements
            XmlNodeList stringElements = toastXml.GetElementsByTagName("text");
            stringElements[0].AppendChild(toastXml.CreateTextNode(line1));

            // Specify the absolute path to an image
            string imagePath = ParseUri(img);
            XmlNodeList imageElements = toastXml.GetElementsByTagName("image");
            imageElements[0].Attributes.GetNamedItem("src").NodeValue = imagePath;

            if (silent)
                MakeSilent(toastXml);

            ShowToast(toastXml);
        }

        public static void ToastToastImageAndText02(string title, string line1, string img)
        {
            // Get a toast XML template
            XmlDocument toastXml = ToastNotificationManager.GetTemplateContent(
                ToastTemplateType.ToastImageAndText02);

            // Fill in the text elements
            XmlNodeList stringElements = toastXml.GetElementsByTagName("text");
            stringElements[0].AppendChild(toastXml.CreateTextNode(title));
            stringElements[1].AppendChild(toastXml.CreateTextNode(line1));

            // Specify the absolute path to an image
            string imagePath = ParseUri(img);
            XmlNodeList imageElements = toastXml.GetElementsByTagName("image");
            imageElements[0].Attributes.GetNamedItem("src").NodeValue = imagePath;

            if (silent)
                MakeSilent(toastXml);

            ShowToast(toastXml);
        }

        public static void ToastToastImageAndText03(string title, string line1, string img)
        {
            // Get a toast XML template
            XmlDocument toastXml = ToastNotificationManager.GetTemplateContent(
                ToastTemplateType.ToastImageAndText03);

            // Fill in the text elements
            XmlNodeList stringElements = toastXml.GetElementsByTagName("text");
            stringElements[0].AppendChild(toastXml.CreateTextNode(title));
            stringElements[1].AppendChild(toastXml.CreateTextNode(line1));

            // Specify the absolute path to an image
            string imagePath = ParseUri(img);
            XmlNodeList imageElements = toastXml.GetElementsByTagName("image");
            imageElements[0].Attributes.GetNamedItem("src").NodeValue = imagePath;

            if (silent)
                MakeSilent(toastXml);

            ShowToast(toastXml);
        }

        public static void ToastToastImageAndText04(string title, string line1, string line2, string img)
        {
            // Get a toast XML template
            XmlDocument toastXml = ToastNotificationManager.GetTemplateContent(
                ToastTemplateType.ToastImageAndText04);

            // Fill in the text elements
            XmlNodeList stringElements = toastXml.GetElementsByTagName("text");
            stringElements[0].AppendChild(toastXml.CreateTextNode(title));
            stringElements[1].AppendChild(toastXml.CreateTextNode(line1));
            stringElements[2].AppendChild(toastXml.CreateTextNode(line2));

            // Specify the absolute path to an image
            string imagePath = ParseUri(img);
            XmlNodeList imageElements = toastXml.GetElementsByTagName("image");
            imageElements[0].Attributes.GetNamedItem("src").NodeValue = imagePath;

            if (silent)
                MakeSilent(toastXml);

            ShowToast(toastXml);
        }

        public static void ToastToastText01(string line1)
        {
            // Get a toast XML template
            XmlDocument toastXml = ToastNotificationManager.GetTemplateContent(
                ToastTemplateType.ToastText01);

            // Fill in the text elements
            XmlNodeList stringElements = toastXml.GetElementsByTagName("text");
            stringElements[0].AppendChild(toastXml.CreateTextNode(line1));

            if (silent)
                MakeSilent(toastXml);

            ShowToast(toastXml);
        }

        public static void ToastToastText02(string title, string line1)
        {
            // Get a toast XML template
            XmlDocument toastXml = ToastNotificationManager.GetTemplateContent(
                ToastTemplateType.ToastText02);

            // Fill in the text elements
            XmlNodeList stringElements = toastXml.GetElementsByTagName("text");
            stringElements[0].AppendChild(toastXml.CreateTextNode(title));
            stringElements[1].AppendChild(toastXml.CreateTextNode(line1));

            if (silent)
                MakeSilent(toastXml);

            ShowToast(toastXml);
        }

        public static void ToastToastText03(string title, string line1)
        {
            // Get a toast XML template
            XmlDocument toastXml = ToastNotificationManager.GetTemplateContent(
                ToastTemplateType.ToastText03);

            // Fill in the text elements
            XmlNodeList stringElements = toastXml.GetElementsByTagName("text");
            stringElements[0].AppendChild(toastXml.CreateTextNode(title));
            stringElements[1].AppendChild(toastXml.CreateTextNode(line1));

            if (silent)
                MakeSilent(toastXml);

            ShowToast(toastXml);
        }

        public static void ToastToastText04(string title, string line1, string line2)
        {
            // Get a toast XML template
            XmlDocument toastXml = ToastNotificationManager.GetTemplateContent(
                ToastTemplateType.ToastText04);

            // Fill in the text elements
            XmlNodeList stringElements = toastXml.GetElementsByTagName("text");
            stringElements[0].AppendChild(toastXml.CreateTextNode(title));
            stringElements[1].AppendChild(toastXml.CreateTextNode(line1));
            stringElements[2].AppendChild(toastXml.CreateTextNode(line2));

            if (silent)
                MakeSilent(toastXml);

            ShowToast(toastXml);
        }

        private static string ParseUri(string uri)
        {
            if (uri.Length >=3 && Char.IsLetter(uri[0]) && uri.Substring(1, 2) == ":/")
                return String.Format(@"file://{0}", uri);
            else
                return uri;
        }

        private static void MakeSilent(XmlDocument toastXml)
        {
            IXmlNode toastNode = toastXml.SelectSingleNode("/toast");
            XmlElement audioNode = toastXml.CreateElement("audio");
            audioNode.SetAttribute("silent", "true");
            toastNode.AppendChild(audioNode);
        }
    }
}

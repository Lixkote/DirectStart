using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Windows.Input;
using System.Windows;
using Microsoft.Win32;

namespace B8TAM.FrequentHelpers
{
    public class CountEntry : INotifyPropertyChanged
    {
        private static Dictionary<char, char> lookupTable = new Dictionary<char, char>()
        {
            {'A', 'N'}, {'a', 'n'},
            {'B', 'O'}, {'b', 'o'},
            {'C', 'P'}, {'c', 'p'},
            {'D', 'Q'}, {'d', 'q'},
            {'E', 'R'}, {'e', 'r'},
            {'F', 'S'}, {'f', 's'},
            {'G', 'T'}, {'g', 't'},
            {'H', 'U'}, {'h', 'u'},
            {'I', 'V'}, {'i', 'v'},
            {'J', 'W'}, {'j', 'w'},
            {'K', 'X'}, {'k', 'x'},
            {'L', 'Y'}, {'l', 'y'},
            {'M', 'Z'}, {'m', 'z'},
            {'N', 'A'}, {'n', 'a'},
            {'O', 'B'}, {'o', 'b'},
            {'P', 'C'}, {'p', 'c'},
            {'Q', 'D'}, {'q', 'd'},
            {'R', 'E'}, {'r', 'e'},
            {'S', 'F'}, {'s', 'f'},
            {'T', 'G'}, {'t', 'g'},
            {'U', 'H'}, {'u', 'h'},
            {'V', 'I'}, {'v', 'i'},
            {'W', 'J'}, {'w', 'j'},
            {'X', 'K'}, {'x', 'k'},
            {'Y', 'L'}, {'y', 'l'},
            {'Z', 'M'}, {'z', 'm'},
        };

        private static Dictionary<string, Environment.SpecialFolder> folderGUID = new Dictionary<string, Environment.SpecialFolder>()
{
    {"{D20BEEC4-5CA8-4905-AE3B-BF251EA09B53}", Environment.SpecialFolder.NetworkShortcuts},
    {"{0AC0837C-BBF8-452A-850D-79D08E667CA7}", Environment.SpecialFolder.MyComputer},
    {"{4D9F7874-4E0C-4904-967B-40B0D20C3E4B}", Environment.SpecialFolder.InternetCache},
    {"{FD228CB7-AE11-4AE3-864C-16F3910AB8FE}", Environment.SpecialFolder.Fonts},
    {"{B4BFCC3A-DB2C-424C-B029-7FE99A87C641}", Environment.SpecialFolder.Desktop},
    {"{B97D20BB-F46A-4C97-BA10-5E3608430854}", Environment.SpecialFolder.Startup},
    {"{A77F5D77-2E2B-44C3-A6A2-ABA601054A51}", Environment.SpecialFolder.Programs},
    {"{625B53C3-AB48-4EC1-BA1F-A1EF4146FC19}", Environment.SpecialFolder.StartMenu},
    {"{AE50C081-EBD2-438A-8655-8A092E34987A}", Environment.SpecialFolder.Recent},
    {"{8983036C-27C0-404B-8F08-102D10DCFD74}", Environment.SpecialFolder.SendTo},
    {"{FDD39AD0-238F-46AF-ADB4-6C85480369C7}", Environment.SpecialFolder.MyDocuments},
    {"{1777F761-68AD-4D8A-87BD-30B759FA33DD}", Environment.SpecialFolder.Favorites},
    {"{A63293E8-664E-48DB-A079-DF759E0509F7}", Environment.SpecialFolder.Templates},
    {"{82A5EA35-D9CD-47C5-9629-E15D2F714E6E}", Environment.SpecialFolder.CommonStartup},
    {"{0139D44E-6AFE-49F2-8690-3DAFCAE6FFB8}", Environment.SpecialFolder.CommonPrograms},
    {"{A4115719-D62E-491D-AA7C-E74B8BE3B067}", Environment.SpecialFolder.CommonStartMenu},
    {"{62AB5D82-FDC1-4DC3-A9DD-070D1D495D97}", Environment.SpecialFolder.CommonApplicationData},
    {"{B94237E7-57AC-4347-9151-B08C6C32D1F7}", Environment.SpecialFolder.CommonTemplates},
    {"{ED4824AF-DCE4-45A8-81E2-FC7965083634}", Environment.SpecialFolder.CommonDocuments},
    {"{3EB685DB-65F9-4CF6-A03A-E3EF65729F3D}", Environment.SpecialFolder.ApplicationData},
    {"{F1B32785-6FBA-4FCF-9D55-7B8E7F157091}", Environment.SpecialFolder.LocalApplicationData},
    {"{352481E8-33BE-4251-BA85-6007CAEDCF9D}", Environment.SpecialFolder.InternetCache},
    {"{2B0F765D-C0E9-4171-908E-08A611B84FF6}", Environment.SpecialFolder.Cookies},
    {"{D9DC8A3B-B784-432E-A781-5A1130A75963}", Environment.SpecialFolder.History},
    {"{1AC14E77-02E7-4E5D-B744-2EB1AE5198B7}", Environment.SpecialFolder.System},
    {"{D65231B0-B2F1-4857-A4CE-A8E7C6EA7D27}", Environment.SpecialFolder.SystemX86},
    {"{F38BF404-1D43-42F2-9305-67DE0B28FC23}", Environment.SpecialFolder.Windows},
    {"{5E6C858F-0E22-4760-9AFE-EA3317B67173}", Environment.SpecialFolder.UserProfile},
    {"{33E28130-4E1E-4676-835A-98395C3BC3BB}", Environment.SpecialFolder.MyPictures},
    {"{7C5A40EF-A0FB-4BFC-874A-C0F2E0B9FA8E}", Environment.SpecialFolder.ProgramFilesX86},
    {"{DE974D24-D9C6-4D3E-BF91-F4455120B917}", Environment.SpecialFolder.CommonProgramFilesX86},
    {"{6D809377-6AF0-444b-8957-A3773F02200E}", Environment.SpecialFolder.ProgramFiles},
    {"{6365D5A7-0F0D-45e5-87F6-0DA56B6A4F7D}", Environment.SpecialFolder.CommonProgramFiles},
    {"{bcbd3057-ca5c-4622-b42d-bc56db0ae516}", Environment.SpecialFolder.CommonProgramFiles},
    {"{724EF170-A42D-4FEF-9F26-B60E846FBA4F}", Environment.SpecialFolder.AdminTools},
    {"{D0384E7D-BAC3-4797-8F14-CBA229B392B5}", Environment.SpecialFolder.CommonAdminTools},
    {"{4BD8D571-6D19-48D3-BE97-422220080E43}", Environment.SpecialFolder.MyMusic},
    {"{18989B1D-99B5-455B-841C-AB7C74E4DDFC}", Environment.SpecialFolder.MyVideos},
    {"{B6EBFB86-6907-413C-9AF7-4FC2ABF07CC5}", Environment.SpecialFolder.CommonPictures},
    {"{3214FAB5-9757-4298-BB61-92A9DEAA44FF}", Environment.SpecialFolder.CommonMusic},
    {"{2400183A-6185-49FB-A2D8-4A392A602BA3}", Environment.SpecialFolder.CommonVideos},
    {"{8AD10C31-2ADB-4296-A8F7-E4701232C972}", Environment.SpecialFolder.Resources},
    {"{2A00375E-224C-49DE-B8D1-440DF7EF3DDC}", Environment.SpecialFolder.LocalizedResources},
    {"{C1BAE2D0-10DF-4334-BEDD-7AA20B227A9D}", Environment.SpecialFolder.CommonOemLinks},
    {"{9E52AB10-F80D-49DF-ACB8-4330F5687855}", Environment.SpecialFolder.CDBurning},
};


        private byte[] value;
        private string name;
        private int executionCount;
        private string regKey;
        private ICommand deleteCommand;
        private ICommand detailCommand;
        public event PropertyChangedEventHandler PropertyChanged;

        public CountEntry()
        {
            this.deleteCommand = new DelegateCommand(OnDelete);
        }

        public byte[] Value
        {
            get
            {
                return this.value;
            }

            set
            {
                this.value = value;
                executionCount = BitConverter.ToInt32(value, 4);
            }
        }

        public string Name
        {
            get { return name; }
            set { name = value; }
        }

        public string RegKey
        {
            get { return regKey; }
            set { regKey = value; }
        }

        public String DecodedName
        {
            get
            {
                if (string.IsNullOrEmpty(Name))
                {
                    return "";
                }
                else
                {
                    string result = new string(Name.ToCharArray().Select(c =>
                    {
                        return lookupTable.ContainsKey(c) ? lookupTable[c] : c;
                    }).ToArray());
                    foreach (var f in folderGUID)
                    {
                        if (result.Contains(f.Key)) result = result.Replace(f.Key, Environment.GetFolderPath(f.Value));
                    }
                    return result;
                }
            }
        }

        public int ExecutionCount
        {
            get { return executionCount; }
            set
            {
                executionCount = value;
                PropertyChanged(this, new PropertyChangedEventArgs("ExecutionCount"));
            }
        }

        public ICommand DeleteCommand
        {
            get { return deleteCommand; }
        }

        public ICommand DetailCommand
        {
            get { return detailCommand; }
        }

        public void OnDelete()
        {
            if (MessageBox.Show("Do you really want delete this entry?", "Delete Confirmation",
                MessageBoxButton.YesNo, MessageBoxImage.Warning) == MessageBoxResult.Yes)
            {
                RegistryKey key = Registry.CurrentUser.OpenSubKey(RegKey.Replace(@"HKEY_CURRENT_USER\", ""), true);
                if (key != null)
                {
                    try
                    {
                        key.DeleteValue(Name);
                        PropertyChanged(this, new PropertyChangedEventArgs("DeleteCommand"));
                    }
                    catch (Exception ex)
                    {
                        MessageBox.Show("Error Deleting Registry: " + ex.Message, "Error", MessageBoxButton.OK, MessageBoxImage.Error);
                    }
                }

            }
        }
    }
}

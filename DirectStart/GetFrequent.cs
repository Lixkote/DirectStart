using System;
using System.Collections.Generic;
using System.Runtime.InteropServices;
using System.Text;
using System.Windows;

namespace B8TAM
{
    class GetFrequent
    {
        [DllImport("shell32.dll")]
        private static extern int SHGetPropertyStoreForWindow(IntPtr hwnd, ref Guid iid, out IPropertyStore propertyStore);

        [DllImport("shell32.dll")]
        private static extern int SHCreateItemFromParsingName([MarshalAs(UnmanagedType.LPWStr)] string pszPath, IntPtr pbc, ref Guid riid, out IShellItem ppv);

        [ComImport]
        [Guid("886D8EEB-8CF2-4446-8D02-CDBA1DBDCF99")]
        [InterfaceType(ComInterfaceType.InterfaceIsIUnknown)]
        interface IPropertyStore
        {
            int GetCount(out uint propertyCount);
            int GetAt(uint propertyIndex, out PROPERTYKEY key);
            int GetValue(ref PROPERTYKEY key, out PROPVARIANT pv);
            int SetValue(ref PROPERTYKEY key, ref PROPVARIANT propvar);
            int Commit();
        }

        [ComImport]
        [Guid("43826D1E-E718-42EE-BC55-A1E261C37BFE")]
        [InterfaceType(ComInterfaceType.InterfaceIsIUnknown)]
        interface IShellItem
        {
            int BindToHandler(IntPtr pbc, ref Guid bhid, ref Guid riid, out IntPtr ppv);
            int GetParent(out IShellItem ppsi);
            int GetDisplayName(SIGDN sigdnName, out IntPtr ppszName);
            int GetAttributes(uint sfgaoMask, out uint psfgaoAttribs);
            int Compare(IShellItem psi, uint hint, out int piOrder);
        }

        [StructLayout(LayoutKind.Sequential, Pack = 4)]
        struct PROPERTYKEY
        {
            public Guid fmtid;
            public uint pid;
        }

        [StructLayout(LayoutKind.Explicit)]
        struct PROPVARIANT
        {
            [FieldOffset(0)]
            public VarEnum vt;
            [FieldOffset(2)]
            public ushort wReserved1;
            [FieldOffset(4)]
            public ushort wReserved2;
            [FieldOffset(6)]
            public ushort wReserved3;
            [FieldOffset(8)]
            public byte bVal;
            [FieldOffset(8)]
            public short iVal;
            [FieldOffset(8)]
            public int intVal;
            [FieldOffset(8)]
            public long hVal;
            [FieldOffset(8)]
            public float fltVal;
            [FieldOffset(8)]
            public double dblVal;
            [FieldOffset(8)]
            public IntPtr pclsidVal;
            [FieldOffset(8)]
            public IntPtr pszVal;
            [FieldOffset(8)]
            public IntPtr pwszVal;
            [FieldOffset(8)]
            public IntPtr punkVal;
            [FieldOffset(8)]
            public System.Runtime.InteropServices.ComTypes.FILETIME filetime;
        }

        enum VarEnum : ushort
        {
            VT_EMPTY = 0,
            VT_NULL = 1,
            VT_I2 = 2,
            VT_I4 = 3,
            VT_R4 = 4,
            VT_R8 = 5,
            VT_CY = 6,
            VT_DATE = 7,
            VT_BSTR = 8,
            VT_DISPATCH = 9,
            VT_ERROR = 10,
            VT_BOOL = 11,
            VT_VARIANT = 12,
            VT_UNKNOWN = 13,
            VT_DECIMAL = 14,
            VT_I1 = 16,
            VT_UI1 = 17,
            VT_UI2 = 18,
            VT_UI4 = 19,
            VT_I8 = 20,
            VT_UI8 = 21,
            VT_INT = 22,
            VT_UINT = 23,
            VT_VOID = 24,
            VT_HRESULT = 25,
            VT_PTR = 26,
            VT_SAFEARRAY = 27,
            VT_CARRAY = 28,
            VT_USERDEFINED = 29,
            VT_LPSTR = 30,
            VT_LPWSTR = 31,
            VT_RECORD = 36,
            VT_INT_PTR = 37,
            VT_UINT_PTR = 38,
            VT_FILETIME = 64,
            VT_BLOB = 65,
            VT_STREAM = 66,
            VT_STORAGE = 67,
            VT_STREAMED_OBJECT = 68,
            VT_STORED_OBJECT = 69,
            VT_BLOB_OBJECT = 70,
            VT_CF = 71,
            VT_CLSID = 72,
            VT_VERSIONED_STREAM = 73,
            VT_BSTR_BLOB = 0xfff,
            VT_VECTOR = 0x1000,
            VT_ARRAY = 0x2000,
            VT_BYREF = 0x4000,
            VT_RESERVED = 0x8000,
            VT_ILLEGAL = 0xffff,
            VT_ILLEGALMASKED = 0xfff,
            VT_TYPEMASK = 0xfff
        }

        enum SIGDN : uint
        {
            NORMALDISPLAY = 0,
            PARENTRELATIVEPARSING = 0x80018001,
            PARENTRELATIVEFORADDRESSBAR = 0x8001c001,
            DESKTOPABSOLUTEPARSING = 0x80028000,
            PARENTRELATIVEEDITING = 0x80031001,
            DESKTOPABSOLUTEEDITING = 0x8004c000,
            FILESYSPATH = 0x80058000,
            URL = 0x80068000
        }

        public static List<string> GetRecentlyUsedPrograms()
        {
            List<string> programs = new List<string>();

            // Get the handle of the WPF application window
            IntPtr mainWindowHandle = new System.Windows.Interop.WindowInteropHelper(Application.Current.MainWindow).Handle;

            // Get the Recently Used Programs property store for the window
            Guid guidPropertyStore = new Guid("886D8EEB-8CF2-4446-8D02-CDBA1DBDCF99");
            IPropertyStore propertyStore;
            int hr = SHGetPropertyStoreForWindow(mainWindowHandle, ref guidPropertyStore, out propertyStore);

            if (hr == 0) // S_OK
            {
                // Get the count of properties in the property store
                uint propertyCount;
                propertyStore.GetCount(out propertyCount);

                // Iterate through each property and get the display name
                for (uint i = 0; i < propertyCount; i++)
                {
                    PROPERTYKEY key;
                    propertyStore.GetAt(i, out key);

                    PROPVARIANT propValue;
                    propertyStore.GetValue(ref key, out propValue);

                    // Check if the property key represents the display name
                    if (key.fmtid == new Guid("F29F85E0-4FF9-1068-AB91-08002B27B3D9") && key.pid == 9)
                    {
                        string displayName = Marshal.PtrToStringUni(propValue.pwszVal);
                        programs.Add(displayName);
                    }
                }
            }

            return programs;
        }
    }
}

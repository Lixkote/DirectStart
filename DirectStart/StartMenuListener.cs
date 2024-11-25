﻿using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Runtime.InteropServices;
using System.Text;
using System.Windows.Forms;
using System.Windows.Input;

namespace B8TAM
{
	internal class StartMenuListener
	{
		public StartMenuListener()
		{
			this.MouseCallback += new HookProc(MouseEvents);
			this.KeyCallback += new HookProc(KeyEvents);
			using (Process process = Process.GetCurrentProcess())
			using (ProcessModule module = process.MainModule)
			{
				IntPtr hModule = GetModuleHandle(module.ModuleName);
				_mouseHook = SetWindowsHookEx(WH_MOUSE_LL, this.MouseCallback, hModule, 0);
				_keyHook = SetWindowsHookEx(WH_KEY_LL, this.KeyCallback, hModule, 0);
			}
		}
		int WH_KEY_LL = 13;
		int WH_MOUSE_LL = 14;
		int HC_ACTION = 0;
		HookProc MouseCallback = null;
		HookProc KeyCallback = null;
		IntPtr _mouseHook = IntPtr.Zero;
		IntPtr _keyHook = IntPtr.Zero;

		public event EventHandler<EventArgs> StartTriggered;

		int MouseEvents(int code, IntPtr wParam, IntPtr lParam)
		{
			if (code < 0)
				return CallNextHookEx(_mouseHook, code, wParam, lParam);

			if (code == this.HC_ACTION)
			{
				if (wParam.ToInt32() == (uint)WM.WM_LBUTTONDOWN)
				{
					MSLLHOOKSTRUCT ms = new MSLLHOOKSTRUCT();
					ms = (MSLLHOOKSTRUCT)Marshal.PtrToStructure(lParam, typeof(MSLLHOOKSTRUCT));
					IntPtr win = WindowFromPoint(ms.pt);
					string title = GetWindowTextRaw(win);
					if (title == "Start")
					{
						StartTriggered(this, null);
						return 1;
					}
				}
			}
			return CallNextHookEx(_mouseHook, code, wParam, lParam);
		}
		HashSet<Keys> pressedKeys = new HashSet<Keys>();
		Stopwatch stopwatch = new Stopwatch();

        int KeyEvents(int code, IntPtr wParam, IntPtr lParam)
        {
            if (code < 0)
                return CallNextHookEx(_mouseHook, code, wParam, lParam);

            if (code == this.HC_ACTION)
            {
                KBDLLHOOKSTRUCT objKeyInfo = (KBDLLHOOKSTRUCT)Marshal.PtrToStructure(lParam, typeof(KBDLLHOOKSTRUCT));

                if (wParam == (IntPtr)0x0100) // Key down
                {
                    pressedKeys.Add(objKeyInfo.key); // Add the key to the set of pressed keys
                    stopwatch.Restart(); // Restart the stopwatch
                }
                else if (wParam == (IntPtr)0x0101) // Key up
                {
                    stopwatch.Stop(); // Stop the stopwatch

                    if (pressedKeys.Count == 1 && (objKeyInfo.key == Keys.LWin || objKeyInfo.key == Keys.RWin))
                    {
                        // Introduce a small delay (e.g., 100 milliseconds) to ensure the key is held for a minimum time

                        if (stopwatch.ElapsedMilliseconds <= 100)
                        {
                            bool anyKeyPressed = false;

                            foreach (Key key in Enum.GetValues(typeof(Key)))
                            {
                                if (key != Key.None && key != Key.LWin && key != Key.RWin && Keyboard.IsKeyDown(key))
                                {
                                    anyKeyPressed = true;
                                    break;
                                }
                            }
                            // Only execute the actions if no other keys are pressed
                            if (!anyKeyPressed)
                            {
                                StartTriggered(this, null);
                            }
                        }
                    }

                    pressedKeys.Clear();
                }
            }

            return CallNextHookEx(_mouseHook, code, wParam, lParam);
        }

		public void Close()
		{
			if (_mouseHook != IntPtr.Zero)
			{
				UnhookWindowsHookEx(_mouseHook);
			}
		}
		public delegate int HookProc(int code, IntPtr wParam, IntPtr lParam);

		[System.Runtime.InteropServices.DllImport("user32.dll", EntryPoint = "SetWindowsHookEx", SetLastError = true)]
		public static extern IntPtr SetWindowsHookEx(int idHook, HookProc lpfn, IntPtr hMod, uint dwThreadId);

		[System.Runtime.InteropServices.DllImport("user32.dll")]
		public static extern int CallNextHookEx(IntPtr hhk, int nCode, IntPtr wParam, IntPtr lParam);

		[System.Runtime.InteropServices.DllImport("kernel32.dll", CharSet = System.Runtime.InteropServices.CharSet.Auto)]
		public static extern IntPtr GetModuleHandle(string lpModuleName);

		[DllImport("user32.dll")]
		static extern IntPtr WindowFromPoint(int xPoint, int yPoint);

		[DllImport("user32.dll")]
		static extern IntPtr WindowFromPoint(POINT Point);

		[DllImport("user32.dll", SetLastError = true)]
		[return: MarshalAs(UnmanagedType.Bool)]
		static extern bool UnhookWindowsHookEx(IntPtr hhk);

		[DllImport("user32.dll", CharSet = CharSet.Auto)]
		static extern IntPtr SendMessage(IntPtr hWnd, UInt32 Msg, IntPtr wParam, [Out] StringBuilder lParam);

		public static string GetWindowTextRaw(IntPtr hwnd)
		{
			StringBuilder sb = new StringBuilder(200);
			SendMessage(hwnd, (int)WM.WM_GETTEXT, (IntPtr)sb.Capacity, sb);
			return sb.ToString();
		}
	}
	[StructLayout(LayoutKind.Sequential)]
	public struct KBDLLHOOKSTRUCT
	{
		public Keys key;
		public int scanCode;
		public int flags;
		public int time;
		public IntPtr extra;
	}
	[StructLayout(LayoutKind.Sequential)]
	public struct MSLLHOOKSTRUCT
	{
		public POINT pt;
		public int mouseData;
		public int flags;
		public int time;
		public UIntPtr dwExtraInfo;
	}
	enum WM : uint
	{
		WM_LBUTTONDOWN = 0x0201,
		WM_GETTEXT = 0x000D,
		WM_GETTEXTLENGTH = 0x000E
	}

	[StructLayout(LayoutKind.Sequential)]
	public struct POINT
	{
		public int X;
		public int Y;

		public POINT(int x, int y)
		{
			this.X = x;
			this.Y = y;
		}
	}
}

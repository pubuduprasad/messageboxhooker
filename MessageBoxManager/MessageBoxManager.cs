using System;
using System.Runtime.InteropServices;
using System.Text;

namespace System.Windows.Forms
{
	
	public class MessageBoxManager
	{
		[DllImport("user32.dll")]
		private static extern IntPtr SendMessage(IntPtr hWnd, int Msg, IntPtr wParam, IntPtr lParam);

		[DllImport("user32.dll")]
		private static extern IntPtr SetWindowsHookEx(int idHook, MessageBoxManager.HookProc lpfn, IntPtr hInstance, int threadId);

		[DllImport("user32.dll")]
		private static extern int UnhookWindowsHookEx(IntPtr idHook);

		[DllImport("user32.dll")]
		private static extern IntPtr CallNextHookEx(IntPtr idHook, int nCode, IntPtr wParam, IntPtr lParam);

		[DllImport("user32.dll", CharSet = CharSet.Unicode, EntryPoint = "GetWindowTextLengthW")]
		private static extern int GetWindowTextLength(IntPtr hWnd);

		[DllImport("user32.dll", CharSet = CharSet.Unicode, EntryPoint = "GetWindowTextW")]
		private static extern int GetWindowText(IntPtr hWnd, StringBuilder text, int maxLength);

		[DllImport("user32.dll")]
		private static extern int EndDialog(IntPtr hDlg, IntPtr nResult);

		[DllImport("user32.dll")]
		private static extern bool EnumChildWindows(IntPtr hWndParent, MessageBoxManager.EnumChildProc lpEnumFunc, IntPtr lParam);

		[DllImport("user32.dll", CharSet = CharSet.Unicode, EntryPoint = "GetClassNameW")]
		private static extern int GetClassName(IntPtr hWnd, StringBuilder lpClassName, int nMaxCount);

		[DllImport("user32.dll")]
		private static extern int GetDlgCtrlID(IntPtr hwndCtl);

		[DllImport("user32.dll")]
		private static extern IntPtr GetDlgItem(IntPtr hDlg, int nIDDlgItem);

		[DllImport("user32.dll", CharSet = CharSet.Unicode, EntryPoint = "SetWindowTextW")]
		private static extern bool SetWindowText(IntPtr hWnd, string lpString);

		static MessageBoxManager()
		{
			MessageBoxManager.hookProc = new MessageBoxManager.HookProc(MessageBoxManager.MessageBoxHookProc);
			MessageBoxManager.enumProc = new MessageBoxManager.EnumChildProc(MessageBoxManager.MessageBoxEnumProc);
			MessageBoxManager.hHook = IntPtr.Zero;
		}

		public static void Register(bool hookinfo = true)   
		{
			bool flag = MessageBoxManager.hHook != IntPtr.Zero;
			if (flag)
			{
				MessageBoxManager.UnhookWindowsHookEx(MessageBoxManager.hHook);
				MessageBoxManager.hHook = IntPtr.Zero;
			}
			if (hookinfo)
			{
				if (MessageBoxManager.hHook != IntPtr.Zero)
				{
					Console.Write("one hook per thread allowed");
				}
				MessageBoxManager.hHook = MessageBoxManager.SetWindowsHookEx(12, MessageBoxManager.hookProc, IntPtr.Zero, AppDomain.GetCurrentThreadId());
			}
		}

		private static IntPtr MessageBoxHookProc(int nCode, IntPtr wParam, IntPtr lParam)
		{
			bool flag = nCode < 0;
			IntPtr result;
			if (flag)
			{
				result = MessageBoxManager.CallNextHookEx(MessageBoxManager.hHook, nCode, wParam, lParam);
			}
			else
			{
				MessageBoxManager.CWPRETSTRUCT cwpretstruct = (MessageBoxManager.CWPRETSTRUCT)Marshal.PtrToStructure(lParam, typeof(MessageBoxManager.CWPRETSTRUCT));
				IntPtr idHook = MessageBoxManager.hHook;
				bool flag2 = cwpretstruct.message == 272U;
				if (flag2)
				{
					MessageBoxManager.GetWindowTextLength(cwpretstruct.hwnd);
					StringBuilder stringBuilder = new StringBuilder(10);
					MessageBoxManager.GetClassName(cwpretstruct.hwnd, stringBuilder, stringBuilder.Capacity);
					bool flag3 = stringBuilder.ToString() == "#32770";
					if (flag3)
					{
						MessageBoxManager.nButton = 0;
						MessageBoxManager.EnumChildWindows(cwpretstruct.hwnd, MessageBoxManager.enumProc, IntPtr.Zero);
						bool flag4 = MessageBoxManager.nButton == 1;
						if (flag4)
						{
							IntPtr dlgItem = MessageBoxManager.GetDlgItem(cwpretstruct.hwnd, 2);
							bool flag5 = dlgItem != IntPtr.Zero;
							if (flag5)
							{
								MessageBoxManager.SetWindowText(dlgItem, MessageBoxManager.OK);
							}
						}
					}
				}
				result = MessageBoxManager.CallNextHookEx(idHook, nCode, wParam, lParam);
			}
			return result;
		}

		private static bool MessageBoxEnumProc(IntPtr hWnd, IntPtr lParam)
		{
			StringBuilder stringBuilder = new StringBuilder(10);
			MessageBoxManager.GetClassName(hWnd, stringBuilder, stringBuilder.Capacity);
			bool flag = stringBuilder.ToString() == "Button";
			if (flag)
			{
				switch (MessageBoxManager.GetDlgCtrlID(hWnd))
				{
				case 1:
						MessageBoxManager.SetWindowText(hWnd, MessageBoxManager.OK);
					break;
				case 2:
						MessageBoxManager.SetWindowText(hWnd, MessageBoxManager.Cancel);
					break;
				case 3:
						MessageBoxManager.SetWindowText(hWnd, MessageBoxManager.Abort);
					break;
				case 4:
						MessageBoxManager.SetWindowText(hWnd, MessageBoxManager.Retry);
					break;
				case 5:
						MessageBoxManager.SetWindowText(hWnd, MessageBoxManager.Ignore);
					break;
				case 6:
						MessageBoxManager.SetWindowText(hWnd, MessageBoxManager.Yes);
					break;
				case 7:
						MessageBoxManager.SetWindowText(hWnd, MessageBoxManager.No);
					break;
				}
				MessageBoxManager.nButton++;
			}
			return true;
		}

		private const int WH_CALLWNDPROCRET = 12;
		private const int WM_DESTROY = 2;
		private const int WM_INITDIALOG = 272;
		private const int WM_TIMER = 275;
		private const int WM_USER = 1024;
		private const int DM_GETDEFID = 1024;
		private const int MBOK = 1;
		private const int MBCancel = 2;
		private const int MBAbort = 3;
		private const int MBRetry = 4;
		private const int MBIgnore = 5;
		private const int MBYes = 6;
		private const int MBNo = 7;
		private static MessageBoxManager.HookProc hookProc;
		private static MessageBoxManager.EnumChildProc enumProc;

		[ThreadStatic]
		private static IntPtr hHook;

		[ThreadStatic]
		private static int nButton;
		public static string OK = "&OK";
		public static string Cancel = "&Cancel";
		public static string Abort = "&Abort";
		public static string Retry = "&Retry";
		public static string Ignore = "&Ignore";
		public static string Yes = "&Yes";
		public static string No = "&No";
		private delegate IntPtr HookProc(int nCode, IntPtr wParam, IntPtr lParam);
		private delegate bool EnumChildProc(IntPtr hWnd, IntPtr lParam);


		public struct CWPRETSTRUCT
		{
			public IntPtr lResult;
			public IntPtr lParam;
			public IntPtr wParam;
			public uint message;
			public IntPtr hwnd;
		}
	}
}

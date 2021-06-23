using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Input;
using System.Windows;
using System.Windows.Interop;

namespace 小小截图OCR {
	[System.Security.SuppressUnmanagedCodeSecurity]
	public class HotKey {
		/// <summary>
		/// 热键消息
		/// </summary>
		const int WM_HOTKEY = 0x312;

		/// <summary>
		/// 注册热键
		/// </summary>
		[DllImport("user32", SetLastError = true)]
		static extern bool RegisterHotKey(IntPtr hWnd, int id, ModifierKeys fsModifuers, Keys vk);

		/// <summary>
		/// 注销热键
		/// </summary>
		[DllImport("user32", SetLastError = true)]
		static extern bool UnregisterHotKey(IntPtr hWnd, int id);

		/// <summary>
		/// 向原子表中添加全局原子
		/// </summary>
		[DllImport("kernel32", SetLastError = true)]
		static extern ushort GlobalAddAtom(string lpString);

		/// <summary>
		/// 在表中搜索全局原子
		/// </summary>
		[DllImport("kernel32", SetLastError = true)]
		static extern ushort GlobalFindAtom(string lpString);

		/// <summary>
		/// 在表中删除全局原子
		/// </summary>
		[DllImport("kernel32", SetLastError = true)]
		static extern ushort GlobalDeleteAtom(string nAtom);

		static string GetString(ModifierKeys modifiers, Keys key)
			=> $"Saar:{modifiers}+{key}";


		private HwndSource source;
		private Action action;

		public ModifierKeys Modifiers { get; }
		public Keys Key { get; }
		public int Id { get; }

		private HotKey(Window window, ModifierKeys modifiers, Keys key, Action action) {
			Modifiers = modifiers;
			Key = key;
			this.action = action;

			try {
				var helper = new WindowInteropHelper(window);
				var hwnd = helper.Handle;
				source = HwndSource.FromHwnd(hwnd);
				source.AddHook(WndProc);
				var strKey = GetString(modifiers, key);
				Id = GlobalFindAtom(strKey);
				if (Id != 0) {
					UnregisterHotKey(hwnd, Id);
				} else {
					Id = GlobalAddAtom(strKey);
				}

				if (!RegisterHotKey(hwnd, Id, modifiers, key))
					throw new Exception();
			} catch (Exception e) {
				throw new HotKeyRegisterFailException(e);
			}
		}

		public static HotKey Register(Window window, ModifierKeys modifiers, Keys key, Action action) {
			return new HotKey(window, modifiers, key, action);
		}

		public void Unregister() {
			UnregisterHotKey(source.Handle, Id);
			GlobalDeleteAtom(GetString(Modifiers, Key));
			source.RemoveHook(WndProc);
		}

		private IntPtr WndProc(IntPtr hwnd, int msg, IntPtr wParam, IntPtr lParam, ref bool handle) {
			if (msg == WM_HOTKEY && (int) wParam == Id) {
				action();
				handle = true;
			}
			return IntPtr.Zero;
		}
	}
}

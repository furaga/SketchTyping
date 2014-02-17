using System;
using System.IO;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using System.Diagnostics;
using System.Runtime.InteropServices;
using System.Management;

namespace FLib
{
    public enum WH
    {
        KEYBOARD_LL = 13,
        MOUSE_LL = 14,
    }
    public enum WM
    {
        KEYDOWN = 0x0100,
        KEYUP = 0x0101,
        SYSKEYDOWN = 0x0104,
        SYSKEYUP = 0x0105,
        MOUSEMOVE = 0x0200,
        LBUTTONDOWN = 0x0201,
        LBUTTONUP = 0x0202,
        LBUTTONDBLCLK = 0x0203,
        RBUTTONDOWN = 0x0204,
        RBUTTONUP = 0x0205,
        RBUTTONDBLCLK = 0x0206,
        MBUTTONDOWN = 0x0207,
        MBUTTONUP = 0x0208,
        MBUTTONDBLCLK = 0x0209,
        MOUSEWHEEL = 0x020A,
        XBUTTONDOWN = 0x020B,
        XBUTTONUP = 0x020C,
        XBUTTONDBLCLK = 0x020D,
        MOUSEHWHEEL = 0x020E,
    }
    public struct KBDLLHOOKSTRUCT
    {
        public int vkCode;
        int scanCode;
        public int flags;
        int time;
        int dwExtraInfo;
    }

    public enum TriggerType { ContextMenu, HotKey }

    delegate int MouseHookHandler(int code, WM message, IntPtr state);
    delegate int KeyHookHandler(int nCode, WM wParam, ref KBDLLHOOKSTRUCT lParam);


    public class Hooker
    {
        [DllImport("user32.dll", SetLastError = true)]
        static extern IntPtr SetWindowsHookEx(WH hookType, MouseHookHandler hookDelegate, IntPtr module, uint threadId);
        [DllImport("user32.dll", EntryPoint = "SetWindowsHookExA", CharSet = CharSet.Ansi)]
        static extern IntPtr SetWindowsHookEx(WH idHook, KeyHookHandler lpfn, IntPtr module, uint dwThreadId);
        [DllImport("user32.dll", SetLastError = true)]
        static extern bool UnhookWindowsHookEx(IntPtr hook);
        [DllImport("user32.dll")]
        static extern int CallNextHookEx(IntPtr hook, int code, WM message, IntPtr state);
        [DllImport("user32.dll", EntryPoint = "CallNextHookEx", CharSet = CharSet.Ansi)]
        static extern int CallNextHookEx(int hook, int code, WM wParam, ref KBDLLHOOKSTRUCT lParam);

        IntPtr mouseHook;
        IntPtr keyHook;
        MouseHookHandler mouseHandler;
        KeyHookHandler keyHandler;
        public bool onCtrl = false;
        public bool onAlt = false;
        public bool onShift = false;
        public bool onFn = false;
        public bool onWin = false;
        
        //-----------------------------------------------------
        // フック開始/終了
        //-----------------------------------------------------

        // キー入力とマウス入力をフック
        public void Hook()
        {
            IntPtr hMod = Marshal.GetHINSTANCE(System.Reflection.Assembly.GetExecutingAssembly().GetModules()[0]);

            UnhookWindowsHookEx(mouseHook);
            UnhookWindowsHookEx(keyHook);
            mouseHandler = new MouseHookHandler(OnMouseLLHook);
            keyHandler = new KeyHookHandler(OnKeyLLHook);
            mouseHook = SetWindowsHookEx(WH.MOUSE_LL, mouseHandler, hMod, 0);
            keyHook = SetWindowsHookEx(WH.KEYBOARD_LL, keyHandler, hMod, 0);

            if (mouseHook == IntPtr.Zero || keyHook == IntPtr.Zero)
            {
                UnhookWindowsHookEx(mouseHook);
                UnhookWindowsHookEx(keyHook);
                int errorCode = Marshal.GetLastWin32Error();
                Console.WriteLine(new Win32Exception(errorCode));
            }
        }

        // キー・マウス入力をアンフック
        public void Unhook()
        {
            UnhookWindowsHookEx(mouseHook);
            UnhookWindowsHookEx(keyHook);
        }

        public Func<int, WM, KBDLLHOOKSTRUCT, Hooker, bool> OnKeyHook = null;

        //-----------------------------------------------------
        // キーのフック処理
        //-----------------------------------------------------
        int OnKeyLLHook(int code, WM wParam, ref KBDLLHOOKSTRUCT lParam)
        {
            UpdateSysKeyStates(wParam, ref lParam);
            bool ignore = false;
            if (OnKeyHook != null)
            {
                ignore = OnKeyHook(code, wParam, lParam, this);
            }
            return ignore ? 1 : CallNextHookEx(0, code, wParam, ref lParam);
        }

        void UpdateSysKeyStates(WM wParam, ref KBDLLHOOKSTRUCT lParam)
        {
            switch (wParam)
            {
                case WM.KEYDOWN:
                case WM.SYSKEYDOWN:
                    if (lParam.vkCode == 160 || lParam.vkCode == 161) onShift = true;
                    if (lParam.vkCode == 162 || lParam.vkCode == 163) onCtrl = true;
                    if (lParam.vkCode == 164 || lParam.vkCode == 165) onAlt = true;
                    break;
                case WM.KEYUP:
                case WM.SYSKEYUP:
                    if (lParam.vkCode == 160 || lParam.vkCode == 161) onShift = false;
                    if (lParam.vkCode == 162 || lParam.vkCode == 163) onCtrl = false;
                    if (lParam.vkCode == 164 || lParam.vkCode == 165) onAlt = false;
                    break;
            }
        }

        //-----------------------------------------------------
        // マウスのフック処理
        //-----------------------------------------------------

        public Action<int, WM, IntPtr, Hooker> OnMouseHook = null;

        int OnMouseLLHook(int code, WM message, IntPtr state)
        {
            try
            {
                if (OnMouseHook != null) OnMouseHook(code, message, state, this);
                return CallNextHookEx(mouseHook, code, message, state);
            }
            catch (Exception e)
            {
                Console.WriteLine(e);
                return CallNextHookEx(mouseHook, code, message, state);
            }
        }
    }


}

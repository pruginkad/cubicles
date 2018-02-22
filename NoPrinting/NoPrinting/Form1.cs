using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Windows.Forms;
using System.Runtime.InteropServices;
using System.Diagnostics;

namespace NoPrinting
{
    public partial class Form1 : Form
    {
        public delegate int HookProc(int nCode, IntPtr wParam, IntPtr lParam);

        //Declare the hook handle as an int.
        static int hHook = 0;

        //Declare the mouse hook constant.
        //For other hook types, you can obtain these values from Winuser.h in the Microsoft SDK.
        public const int WH_KEYBOARD_LL = 13;

        //Declare MouseHookProcedure as a HookProc type.
        HookProc KbHookProcedureDelegate;

        //Declare the wrapper managed POINT class.
        [StructLayout(LayoutKind.Sequential)]
        public class POINT
        {
            public int x;
            public int y;
        }

        //Declare the wrapper managed MouseHookStruct class.
        [StructLayout(LayoutKind.Sequential)]
        public class MouseHookStruct
        {
            public POINT pt;
            public int hwnd;
            public int wHitTestCode;
            public int dwExtraInfo;
        }

        [StructLayout(LayoutKind.Sequential)]
        public class KBDLLHOOKSTRUCT
        {
            public Int32 vkCode;
            public Int32 scanCode;
            public Int32 flags;
            public Int32 time;
            public IntPtr dwExtraInfo;
        }

        //This is the Import for the SetWindowsHookEx function.
        //Use this function to install a thread-specific hook.
        [DllImport("user32.dll", CharSet = CharSet.Auto,
         CallingConvention = CallingConvention.StdCall)]
        public static extern int SetWindowsHookEx(int idHook, HookProc lpfn,
        IntPtr hInstance, int threadId);

        //This is the Import for the UnhookWindowsHookEx function.
        //Call this function to uninstall the hook.
        [DllImport("user32.dll", CharSet = CharSet.Auto,
         CallingConvention = CallingConvention.StdCall)]
        public static extern bool UnhookWindowsHookEx(int idHook);

        //This is the Import for the CallNextHookEx function.
        //Use this function to pass the hook information to the next hook procedure in chain.
        [DllImport("user32.dll", CharSet = CharSet.Auto,
         CallingConvention = CallingConvention.StdCall)]
        public static extern int CallNextHookEx(int idHook, int nCode,
        IntPtr wParam, IntPtr lParam);

        [DllImport("user32.dll", CharSet = CharSet.Auto, ExactSpelling = true, CallingConvention = CallingConvention.Winapi)]
        public static extern short GetAsyncKeyState(int keyCode);

        //Modifier key vkCode constants
        private const int VK_SHIFT = 0x10;
        private const int VK_CONTROL = 0x11;
        private const int VK_MENU = 0x12;
        private const int VK_CAPITAL = 0x14;

        /// <summary>
        /// /////////////////////////////////////////////////////
        /// </summary>
        /// 

        

        [DllImport("user32.dll")]
        private static extern IntPtr GetForegroundWindow();


        [DllImport("user32.dll")]
        static extern uint GetWindowThreadProcessId(IntPtr hWnd, out uint lpdwProcessId);

        [DllImport("kernel32.dll")]
        static extern IntPtr OpenProcess(UInt32 dwDesiredAccess, Int32 bInheritHandle, UInt32 dwProcessId);

        [DllImport("psapi.dll")]
        static extern uint GetModuleFileNameEx(IntPtr hProcess, IntPtr hModule, [Out] StringBuilder lpBaseName, [In] [MarshalAs(UnmanagedType.U4)] int nSize);

        [DllImport("kernel32.dll")]
        [return: MarshalAs(UnmanagedType.Bool)]
        static extern bool CloseHandle(IntPtr hObject);

        [DllImport("user32.dll", SetLastError = true, CharSet = CharSet.Auto)]
        static extern int GetClassName(IntPtr hWnd, StringBuilder lpClassName, int nMaxCount);

        static string GetWindowClassName(IntPtr hWnd)
        {
            const int nChars = 1024;
            StringBuilder filename = new StringBuilder(nChars);
            GetClassName(hWnd, filename, nChars);
            return (filename.ToString().ToLower());
        }
        static string GetWindowModuleFileName(IntPtr hWnd)
        {
            uint processId = 0;
            const int nChars = 1024;
            StringBuilder filename = new StringBuilder(nChars);
            GetWindowThreadProcessId(hWnd, out processId);
            IntPtr hProcess = OpenProcess(1040, 0, processId);
            GetModuleFileNameEx(hProcess, IntPtr.Zero, filename, nChars);
            CloseHandle(hProcess);
            return (filename.ToString().ToLower());
        }

        static  string GetActiveProcessName()
        {
            return GetWindowModuleFileName(GetForegroundWindow());
            //uint processID = 0;
            //IntPtr threadID = GetWindowThreadProcessId(GetForegroundWindow(), out processID);
            //StringBuilder exePath = new StringBuilder(1024);
            //IntPtr hProcess = OpenProcess(1040, 0, processID);
            //uint exePathLen = GetModuleFileName((IntPtr)hProcess, exePath, exePath.Capacity);
            //CloseHandle(hProcess);
            //return exePath.ToString().ToLower();

        }
        public Form1()
        {
            InitializeComponent();
            Hook();
            Visible = false;
        }

        void Hook()
        {
            // Create an instance of HookProc.
            KbHookProcedureDelegate = new HookProc(Form1.KbHookProcedure);

            hHook = SetWindowsHookEx(WH_KEYBOARD_LL,
            KbHookProcedure,
            (IntPtr)0,
            0);
            //If the SetWindowsHookEx function fails.
            if (hHook == 0)
            {
                MessageBox.Show("SetWindowsHookEx Failed");
                return;
            }
        }

        void Unhook()
        {
            bool ret = UnhookWindowsHookEx(hHook);
            //If the UnhookWindowsHookEx function fails.
            if (ret == false)
            {
                MessageBox.Show("UnhookWindowsHookEx Failed");
                return;
            }
            hHook = 0;
        }

        private void button1_Click(object sender, System.EventArgs e)
        {
            Visible = false;
        }

        const int WM_KEYDOWN = 0x0100;
        const int WM_SYSKEYDOWN = 0x0104;

        public static int KbHookProcedure(int nCode, IntPtr wParam, IntPtr lParam)
        {
            //Marshall the data from the callback.
            
            if (nCode >= 0 && wParam == (IntPtr)WM_KEYDOWN)
            {
                KBDLLHOOKSTRUCT kbd = (KBDLLHOOKSTRUCT)Marshal.PtrToStructure(lParam, typeof(KBDLLHOOKSTRUCT));
                if(kbd.vkCode == 80 && (GetAsyncKeyState(VK_CONTROL)&0x8000) != 0)
		        {
                    string process = GetActiveProcessName();
                    if (process.Contains("chrome") ||
                        process.Contains("iexplore") ||
                        process.Contains("opera") ||
                        process.Contains("firefox"))
                    {
                        Application.OpenForms[0].Visible = true;
                        Application.OpenForms[0].WindowState = FormWindowState.Normal;
                        return 1;			        
                    }                    
		        }
            }
            return CallNextHookEx(hHook, nCode, wParam, lParam);
        }

        bool bTerminate = false;
        private void Form1_FormClosing(object sender, FormClosingEventArgs e)
        {
            if (!bTerminate)
                e.Cancel = true;
        }

        private void Form1_FormClosed(object sender, FormClosedEventArgs e)
        {
            Unhook();
        }

        private void Form1_Load(object sender, EventArgs e)
        {
            Visible = false;
            Hide();
            SetVisibleCore(false);
        }

        private void button2_Click(object sender, EventArgs e)
        {
            bTerminate = true;
            Close();
        }
    }
}

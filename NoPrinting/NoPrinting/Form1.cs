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
using System.Drawing.Imaging;
using OCRLib;

namespace NoPrinting
{
    public partial class Form1 : Form
    {
        public delegate int HookProc(int nCode, IntPtr wParam, IntPtr lParam);
        public delegate int MouseHookProc(int nCode, IntPtr wParam, IntPtr lParam);

        //Declare the hook handle as an int.
        static int hHook = 0;
        static int hMouseHook = 0;

        //Declare the mouse hook constant.
        //For other hook types, you can obtain these values from Winuser.h in the Microsoft SDK.
        public const int WH_KEYBOARD_LL = 13;
        private const int WH_MOUSE_LL = 14;

        //Declare MouseHookProcedure as a HookProc type.
        HookProc KbHookProcedureDelegate;
        HookProc MouseHookProcedureDelegate;

        //Declare the wrapper managed POINT class.
        [StructLayout(LayoutKind.Sequential)]
        public struct POINT
        {
            public int x;
            public int y;
        }

        //Declare the wrapper managed MouseHookStruct class.
        
        [StructLayout(LayoutKind.Sequential)]
        public struct MSLLHOOKSTRUCT
        {
            public POINT pt;
            public uint mouseData;
            public uint flags;
            public uint time;
            public IntPtr dwExtraInfo;
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

        [DllImport("user32.dll")]
        static extern IntPtr WindowFromPoint(POINT Point);

        [DllImport("user32.dll")]
        public static extern bool GetWindowRect(IntPtr hWnd, out RECT lpRect);
        [DllImport("user32.dll")]
        public static extern bool PrintWindow(IntPtr hWnd, IntPtr hdcBlt, int nFlags);

        [DllImport("user32.dll", CharSet = CharSet.Auto)]
        private static extern IntPtr SendMessage(IntPtr hWnd, UInt32 Msg, IntPtr wParam, IntPtr lParam);

        private const UInt32 WM_CLOSE = 0x0010;

        public static Bitmap PrintWindow(IntPtr hwnd)
        {
            RECT rc;
            GetWindowRect(hwnd, out rc);

            Bitmap bmp = new Bitmap(rc.Width, rc.Height, PixelFormat.Format32bppArgb);
            Graphics gfxBmp = Graphics.FromImage(bmp);
            IntPtr hdcBitmap = gfxBmp.GetHdc();

            PrintWindow(hwnd, hdcBitmap, 0);

            gfxBmp.ReleaseHdc(hdcBitmap);
            gfxBmp.Dispose();

            return bmp;
        }

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

            if (filename.ToString().ToLower().Contains("applicationframehost"))
            {
                var allChildWindows = new WindowHandleInfo(hWnd).GetAllChildHandles();
                foreach (IntPtr hwnd_child in allChildWindows)
                {
                    uint child_processId = 0;
                    GetWindowThreadProcessId(hwnd_child, out child_processId);
                    if (child_processId != processId)
                    {
                        return GetWindowModuleFileName(hwnd_child);
                    }
                }
            }
            return (filename.ToString().ToLower());
        }

        private enum MouseMessages
        {
            WM_LBUTTONDOWN = 0x0201,
            WM_LBUTTONUP = 0x0202,
            WM_MOUSEMOVE = 0x0200,
            WM_MOUSEWHEEL = 0x020A,
            WM_RBUTTONDOWN = 0x0204,
            WM_RBUTTONUP = 0x0205
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
            if (DesignMode)
            {
                return;
            }
            
            Hook();
            TopMost = true;
            Visible = false;
            WindowState = FormWindowState.Minimized;
        }

        void Hook()
        {
            KbHookProcedureDelegate = new HookProc(Form1.KbHookProcedure);
            hHook = SetWindowsHookEx(WH_KEYBOARD_LL, KbHookProcedureDelegate, (IntPtr)0, 0);

            if (hHook == 0)
            {
                MessageBox.Show("SetWindowsHookEx WH_KEYBOARD_LL Failed");
            }

            MouseHookProcedureDelegate = new HookProc(this.MouseHookProcedure);
            hMouseHook = SetWindowsHookEx(WH_MOUSE_LL, MouseHookProcedureDelegate, (IntPtr)0, 0);

            if (hMouseHook == 0)
            {
                MessageBox.Show("SetWindowsHookEx WH_MOUSE_LL Failed");
            }

        }

        void Unhook()
        {
            bool ret = UnhookWindowsHookEx(hHook);
            
            if (ret == false)
            {
                //MessageBox.Show("UnhookWindowsHookEx Failed");
            }
            hHook = 0;

            ret = UnhookWindowsHookEx(hMouseHook);
            if (ret == false)
            {
                MessageBox.Show("UnhookWindowsHookEx Failed");
            }
            hMouseHook = 0;
        }

        private void button1_Click(object sender, System.EventArgs e)
        {
            Visible = false;
        }

        static bool OurProc(string process)
        {
            if (process.Contains("chrome") ||
                    process.Contains("iexplore") ||
                        process.Contains("edge") ||
                            process.Contains("opera") ||
                                process.Contains("firefox"))
            {
                return true;
            }
            return false;
        }
        const int WM_KEYDOWN = 0x0100;
        const int WM_SYSKEYDOWN = 0x0104;
        const int WM_DESTROY = 0x0002;

        public static int KbHookProcedure(int nCode, IntPtr wParam, IntPtr lParam)
        {
            //Marshall the data from the callback.
            try
            {
                if (nCode >= 0 && wParam == (IntPtr)WM_KEYDOWN)
                {
                    KBDLLHOOKSTRUCT kbd = (KBDLLHOOKSTRUCT)Marshal.PtrToStructure(lParam, typeof(KBDLLHOOKSTRUCT));
                    if (kbd.vkCode == 80 && (GetAsyncKeyState(VK_CONTROL) & 0x8000) != 0)
                    {
                        string process = GetActiveProcessName();
                        if(OurProc(process))
                        {
                            Application.OpenForms[0].Visible = true;
                            Application.OpenForms[0].WindowState = FormWindowState.Normal;
                            return 1;
                        }
                    }
                }
            }
            catch
            {

            }
            
            return CallNextHookEx(hHook, nCode, wParam, lParam);
        }

        private Bitmap GetWindowScreenshot(IntPtr hwnd, POINT pt)
        {
            RECT rc;
            GetWindowRect(hwnd, out rc);

            int width = rc.Right - rc.Left;
            int height = SystemFonts.MenuFont.Height * 3;//rc.Bottom- rc.Top;
            Bitmap bmp = new Bitmap(width, height, PixelFormat.Format32bppArgb);
            Graphics.FromImage(bmp).CopyFromScreen(rc.Left,
                                                   pt.y - height/2,
                                                   0,
                                                   0,
                                                   new Size(width, height),
                                                   CopyPixelOperation.SourceCopy);
            return bmp;
        }

        bool m_bRightClickHappen = false;

        public int MouseHookProcedure(int nCode, IntPtr wParam, IntPtr lParam)
        {
            //Marshall the data from the callback.
            try
            {
                //Marshall the data from the callback.
                MSLLHOOKSTRUCT MyMouseHookStruct = (MSLLHOOKSTRUCT)Marshal.PtrToStructure(lParam, typeof(MSLLHOOKSTRUCT));
                if (nCode >= 0 && MouseMessages.WM_RBUTTONUP == (MouseMessages)wParam)
                {
                    m_bRightClickHappen = false;
                    IntPtr curHwnd = WindowFromPoint(MyMouseHookStruct.pt);
                    if(curHwnd != IntPtr.Zero)
                    {
                        string process = GetWindowModuleFileName(curHwnd);
                        if (OurProc(process))
                        {
                            m_bRightClickHappen = true;
                        }
                    }
                    
                }

                if (m_bRightClickHappen && nCode >= 0 && MouseMessages.WM_LBUTTONUP == (MouseMessages)wParam)
                {
                    //Create a string variable that shows the current mouse coordinates.
                    String strCaption = "x = " +
                    MyMouseHookStruct.pt.x.ToString("d") +
                    "  y = " +
                    MyMouseHookStruct.pt.y.ToString("d");
                    //You must get the active form because it is a static function.
                    Form tempForm = Application.OpenForms[0];

                    //Set the caption of the form.
                    //tempForm.Text = strCaption;
                    IntPtr curHwnd = WindowFromPoint(MyMouseHookStruct.pt);
                    if(curHwnd != IntPtr.Zero)
                    {
                        Bitmap bmp = GetWindowScreenshot(curHwnd, MyMouseHookStruct.pt);
                        pictureBox1.Image = bmp;
                        if (FindPrintWord(bmp))
                        {
                            //SendMessage(curHwnd, WM_DESTROY, IntPtr.Zero, IntPtr.Zero);
                            Application.OpenForms[0].Visible = true;
                            Application.OpenForms[0].WindowState = FormWindowState.Normal;

                            return 1;
                        }
                                             
                    }
                    else
                    {
                        //tempForm.Text = "Unable";
                    }
                    
                    m_bRightClickHappen = false;
                }
            }
            catch(Exception ex)
            {

            }

            return CallNextHookEx(hMouseHook, nCode, wParam, lParam);
        }


        Bitmap CreatePrintTemplate(int h)
        {
            Bitmap printBmp = null;
            try
            {
                string strPrint = "Print";
                printBmp = new Bitmap(400, 100);
                Graphics g = Graphics.FromImage(printBmp);
                SizeF stringSize = new SizeF();
                Font font = new Font(SystemFonts.MenuFont.FontFamily, h);
                stringSize = g.MeasureString(strPrint, font);
                printBmp = new Bitmap((int)stringSize.Width, (int)stringSize.Height);
                g = Graphics.FromImage(printBmp);
                g.FillRectangle(new SolidBrush(SystemColors.MenuHighlight), 0, 0, printBmp.Width, printBmp.Height);
                g.DrawString(strPrint, font, new SolidBrush(SystemColors.MenuText), 0, 0);
                g.Flush();
            }
            catch (Exception ex)
            {
                return null;
            }

            return printBmp;
        }

        Recognizer recogn = new Recognizer();

        bool FindPrintWord(Bitmap bmp_menu)
        {
            recogn.LoadBmp(bmp_menu);

            List<string> arrAddLetters = new List<string>();
            arrAddLetters.Add("ri");
            CharRow row = recogn.Recognize("Print", arrAddLetters);

            if (row != null)
            {
                //Pen pen = new Pen(new SolidBrush(Color.Red));
                //Pen pen_green = new Pen(new SolidBrush(Color.Green));

                //pen.DashStyle = System.Drawing.Drawing2D.DashStyle.Dot;
                //pen_green.DashStyle = System.Drawing.Drawing2D.DashStyle.Dot;

                //Graphics gr = Graphics.FromImage(bmp_menu);
                //gr.DrawRectangle(pen_green, row.m_FullRect);
                return true;
            }
            return false;
        }

        bool bTerminate = false;
        private void Form1_FormClosing(object sender, FormClosingEventArgs e)
        {
            if (!bTerminate)
                e.Cancel = true;
        }

        private void Form1_FormClosed(object sender, FormClosedEventArgs e)
        {
            if (!DesignMode)
            {
                Unhook();
            }            
        }

        private void Form1_Load(object sender, EventArgs e)
        {
            //Visible = false;
            //Hide();
            //SetVisibleCore(false);
        }

        private void button2_Click(object sender, EventArgs e)
        {
            bTerminate = true;
            Close();
        }


        //////////////////////////////////////////
        //////////////////////////////////////////
    }
}

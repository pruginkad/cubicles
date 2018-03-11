using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Windows.Forms;
using System.Drawing.Imaging;
using System.Drawing.Drawing2D;
using System.Drawing.Text;
using System.Runtime.InteropServices;

namespace OCRLib
{
    public partial class Form1 : Form
    {
        public Form1()
        {
            InitializeComponent();
        }

        [StructLayout(LayoutKind.Sequential)]
        public struct POINT
        {
            public int x;
            public int y;
        }

        [StructLayout(LayoutKind.Sequential)]
        public struct RECT
        {
            private int _Left;
            private int _Top;
            private int _Right;
            private int _Bottom;

            public RECT(RECT Rectangle)
                : this(Rectangle.Left, Rectangle.Top, Rectangle.Right, Rectangle.Bottom)
            {
            }
            public RECT(int Left, int Top, int Right, int Bottom)
            {
                _Left = Left;
                _Top = Top;
                _Right = Right;
                _Bottom = Bottom;
            }

            public int X
            {
                get { return _Left; }
                set { _Left = value; }
            }
            public int Y
            {
                get { return _Top; }
                set { _Top = value; }
            }
            public int Left
            {
                get { return _Left; }
                set { _Left = value; }
            }
            public int Top
            {
                get { return _Top; }
                set { _Top = value; }
            }
            public int Right
            {
                get { return _Right; }
                set { _Right = value; }
            }
            public int Bottom
            {
                get { return _Bottom; }
                set { _Bottom = value; }
            }
            public int Height
            {
                get { return _Bottom - _Top; }
                set { _Bottom = value + _Top; }
            }
            public int Width
            {
                get { return _Right - _Left; }
                set { _Right = value + _Left; }
            }
            public Point Location
            {
                get { return new Point(Left, Top); }
                set
                {
                    _Left = value.X;
                    _Top = value.Y;
                }
            }
            public Size Size
            {
                get { return new Size(Width, Height); }
                set
                {
                    _Right = value.Width + _Left;
                    _Bottom = value.Height + _Top;
                }
            }

            public static implicit operator Rectangle(RECT Rectangle)
            {
                return new Rectangle(Rectangle.Left, Rectangle.Top, Rectangle.Width, Rectangle.Height);
            }
            public static implicit operator RECT(Rectangle Rectangle)
            {
                return new RECT(Rectangle.Left, Rectangle.Top, Rectangle.Right, Rectangle.Bottom);
            }
            public static bool operator ==(RECT Rectangle1, RECT Rectangle2)
            {
                return Rectangle1.Equals(Rectangle2);
            }
            public static bool operator !=(RECT Rectangle1, RECT Rectangle2)
            {
                return !Rectangle1.Equals(Rectangle2);
            }

            public override string ToString()
            {
                return "{Left: " + _Left + "; " + "Top: " + _Top + "; Right: " + _Right + "; Bottom: " + _Bottom + "}";
            }

            public override int GetHashCode()
            {
                return ToString().GetHashCode();
            }

            public bool Equals(RECT Rectangle)
            {
                return Rectangle.Left == _Left && Rectangle.Top == _Top && Rectangle.Right == _Right && Rectangle.Bottom == _Bottom;
            }

            public override bool Equals(object Object)
            {
                if (Object is RECT)
                {
                    return Equals((RECT)Object);
                }
                else if (Object is Rectangle)
                {
                    return Equals(new RECT((Rectangle)Object));
                }

                return false;
            }
        }

        [DllImport("user32.dll")]
        static extern IntPtr WindowFromPoint(POINT Point);

        [DllImport("user32.dll")]
        public static extern bool GetWindowRect(IntPtr hWnd, out RECT lpRect);

        private Bitmap GetWindowScreenshot(IntPtr hwnd, POINT pt)
        {
            RECT rc;
            GetWindowRect(hwnd, out rc);

            int width = rc.Right - rc.Left;
            int height = SystemFonts.MenuFont.Height * 3;//rc.Bottom- rc.Top;
            Bitmap bmp = new Bitmap(width, height, PixelFormat.Format32bppArgb);
            Graphics.FromImage(bmp).CopyFromScreen(rc.Left,
                                                   pt.y - height / 2,
                                                   0,
                                                   0,
                                                   new Size(width, height),
                                                   CopyPixelOperation.SourceCopy);
            return bmp;
        }

        Recognizer recogn = new Recognizer();

        private void Form1_Load(object sender, EventArgs e)
        {
            Bitmap bmp_menu = new Bitmap(@"F:\menu_ie.bmp");
            Recognize(bmp_menu);
        }

        void Recognize(Bitmap bmp_menu)
        {
            recogn.LoadBmp(bmp_menu);

            List<string> arrAddLetters = new List<string>();
            arrAddLetters.Add("ri");
            CharRow row = recogn.Recognize("Print", arrAddLetters);

            if (row != null)
            {
                //Rectangle accRect = new Rectangle(0, bmp_menu.Height/3, bmp_menu.Width, bmp_menu.Height*2/3);
                //if (accRect.Contains(row.m_rect))
                {
                    Pen pen = new Pen(new SolidBrush(Color.Red));
                    Pen pen_green = new Pen(new SolidBrush(Color.Green));

                    pen.DashStyle = System.Drawing.Drawing2D.DashStyle.Dot;
                    pen_green.DashStyle = System.Drawing.Drawing2D.DashStyle.Dot;

                    Graphics gr = Graphics.FromImage(bmp_menu);
                    gr.DrawRectangle(pen_green, row.m_FullRect);
                }                
            }

            //pictureBox1.Image = recogn.m_bw;
            pictureBox1.Image = bmp_menu;
        }

        Point GetClick()
        {
            if (pictureBox1.Image == null)
            {
                return Point.Empty;
            }
            Point e = pictureBox1.PointToClient(Cursor.Position);

            float ratioX = (float)(pictureBox1.Image.Width) / (float)(pictureBox1.ClientSize.Width);
            float ratioY = (float)(pictureBox1.Image.Height) / (float)(pictureBox1.ClientSize.Height);


            Point unscaled_p = new Point((int)(e.X * ratioX), (int)(e.Y*ratioY));

            return unscaled_p;
        }

        private void pictureBox1_Click(object sender, EventArgs e)
        {
            MouseEventArgs me = (MouseEventArgs)e;
            Point coordinates = me.Location;

            Point clck = GetClick();
            Bitmap templ = null;
            Bitmap orig = null;
            recogn.RecognizeByPoint(clck,"ri", out templ, out orig);
            pictureBox2.Image = templ;
            pictureBox3.Image = orig;

            templ.Save(@"F:\bmp_templ.bmp");
            orig.Save(@"F:\bmp_orig.bmp");
        }

        private void timer1_Tick(object sender, EventArgs e)
        {
            float brightness_white = (float)0.0;
            POINT pt = new POINT() { x = Cursor.Position.X, y = Cursor.Position.Y };
            IntPtr curHwnd = WindowFromPoint(pt);
            if (curHwnd != IntPtr.Zero)
            {
                Bitmap bmp = GetWindowScreenshot(curHwnd, pt);
                //Cut Selection
                for (int y = 0 ; y < bmp.Height; y++)
                {
                    int x = bmp.Width / 2;
                    Color clr = bmp.GetPixel(x, y);
                    if (clr.GetBrightness() > brightness_white)
                    {
                        brightness_white = clr.GetBrightness();
                    }
                }

                brightness_white = brightness_white - (float)0.01;

                int top = 0;
                int bottom = bmp.Height - 1;
                for (int y = bmp.Height/2; y >= 0; y--)
                {
                    int line_empty_weight = 0;
                    for (int x = 0; x < bmp.Width; x++)
                    {
                        Color clr = bmp.GetPixel(x, y);
                        if (clr.GetBrightness() > brightness_white)
                        {
                            line_empty_weight++;
                        }
                    }
                    if (line_empty_weight > bmp.Width / 2)
                    {
                        top = y;
                        break;
                    }
                }

                for (int y = bmp.Height / 2; y < bmp.Height; y++)
                {
                    int line_empty_weight = 0;
                    for (int x = 0; x < bmp.Width; x++)
                    {
                        Color clr = bmp.GetPixel(x, y);
                        if (clr.GetBrightness() > brightness_white)
                        {
                            line_empty_weight++;
                        }
                    }
                    if (line_empty_weight > bmp.Width * 5 / 6)
                    {
                        bottom = y;
                        break;
                    }
                }
                Rectangle rect = Rectangle.FromLTRB(0, top, bmp.Width, bottom);
                if (rect.Height < 3)
                {
                    //pictureBox1.Image = bmp;
                    //return;
                }
                //bmp = bmp.Clone(rect, bmp.PixelFormat);
                Recognize(bmp);
                Pen pen = new Pen(new SolidBrush(Color.Red));
                pen.DashStyle = System.Drawing.Drawing2D.DashStyle.Dot;
                Graphics gr = Graphics.FromImage(bmp);
                gr.DrawRectangle(pen, rect);
                gr.DrawLine(new Pen(Color.YellowGreen), new Point(0, bmp.Height / 2), new Point(bmp.Width, bmp.Height / 2));
                pictureBox1.Image = bmp;
            }
        }
    }
}

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

        [DllImport("user32.dll")]
        static extern bool ScreenToClient(IntPtr hWnd, ref POINT lpPoint);

        private Bitmap GetWindowScreenshot(IntPtr hwnd, POINT pt)
        {
            RECT rc;
            GetWindowRect(hwnd, out rc);

            
            int width = rc.Right - rc.Left;
            int height = SystemFonts.MenuFont.Height * 3;//rc.Bottom- rc.Top;
            int top = Math.Max(pt.y - height / 2, rc.Top);
            int bottom = Math.Min(top+height, rc.Bottom);
            height = bottom - top;

            Bitmap bmp = new Bitmap(width, height, PixelFormat.Format32bppArgb);
            Graphics.FromImage(bmp).CopyFromScreen(rc.Left,
                                                   top,
                                                   0,
                                                   0,
                                                   new Size(width, height),
                                                   CopyPixelOperation.SourceCopy);

            return bmp;
        }

        Recognizer recogn = new Recognizer();

        private void Form1_Load(object sender, EventArgs e)
        {
            try
            {
                Bitmap bmp_menu = new Bitmap(@"F:\menu_ie.bmp");
                Recognize(bmp_menu);
            }
            catch
            {

            }
        }

        void Recognize(Bitmap bmp_menu_in)
        {
            Pen pen = new Pen(new SolidBrush(Color.Red));
            Pen pen_green = new Pen(new SolidBrush(Color.Green));
            Pen pen_yell = new Pen(new SolidBrush(Color.Yellow));
            
            pen.DashStyle = System.Drawing.Drawing2D.DashStyle.Dot;
            pen_green.DashStyle = System.Drawing.Drawing2D.DashStyle.Dot;

            List<string> arrAddLetters = new List<string>();
            arrAddLetters.Add("Pr");
            arrAddLetters.Add("ri");
            arrAddLetters.Add("nt");

            listBox1.Items.Clear();
            Bitmap bmp_menu = null;

            for (int k = 0; k < 2; k++)
            {
                bmp_menu = bmp_menu_in.Clone(new Rectangle(0, 0, bmp_menu_in.Width, bmp_menu_in.Height), bmp_menu_in.PixelFormat);
                Graphics gr = Graphics.FromImage(bmp_menu);

                recogn.LoadBmp(bmp_menu, k == 1);

                CharRow row = recogn.Recognize("Print", arrAddLetters);


                gr.DrawRectangle(pen_yell, recogn.m_cut_menu_rect);
                for (int i = 0; i < recogn.lines.Count; i++)
                {
                    gr.DrawLine(pen_green, 0, recogn.lines[i], bmp_menu.Width - 1, recogn.lines[i]);
                }

                if (row != null)
                {
                    gr.DrawRectangle(pen, row.m_FullRect);
                    gr.Flush();
                    break;
                }
                else
                {
                    string word = string.Empty;
                    for (int i = 0; i < recogn.chars_row.Count; i++)
                    {
                        CharRow letters = recogn.chars_row[i];
                        for (int j = 0; j < letters.Count; j++)
                        {
                            CharRect letter = letters[j];
                            gr.DrawRectangle(pen, letter.m_rect);

                            word += letter.LetterString;
                        }
                        listBox1.Items.Add(word);
                    }
                    gr.Flush();
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
            
            List<string> arrAddLetters = new List<string>();
            //arrAddLetters.Add("Pr");
            //arrAddLetters.Add("ri");
            //arrAddLetters.Add("nt");
            CharRow row = recogn.Recognize("Print", arrAddLetters, clck);

            pictureBox2.Image = recogn.m_bmp_template;
            pictureBox3.Image = recogn.m_bmp_original;

#if DEBUG
            if (recogn.m_bmp_template != null)
            {
                recogn.m_bmp_template.Save(@"f:\bmp_template.bmp");
            }
            if (recogn.m_bmp_original != null)
            {
                recogn.m_bmp_original.Save(@"f:\bmp_original.bmp");
            }            
#endif
        }

        private void timer1_Tick(object sender, EventArgs e)
        {
            //return;
            POINT pt = new POINT() { x = Cursor.Position.X, y = Cursor.Position.Y };
            IntPtr curHwnd = WindowFromPoint(pt);
            if (curHwnd != IntPtr.Zero)
            {
                Bitmap bmp = GetWindowScreenshot(curHwnd, pt);
                
                
                Recognize(bmp);

                ScreenToClient(curHwnd, ref pt);
                try
                {
                    Color clr = bmp.GetPixel(pt.x, bmp.Height / 2);
                    float bright = clr.GetBrightness();
                    Text = bright.ToString() + "---" + recogn.brightness_white.ToString();
                }
                catch
                {

                }
                

                //Pen pen = new Pen(new SolidBrush(Color.Red));
                //pen.DashStyle = System.Drawing.Drawing2D.DashStyle.Dot;
                //Graphics gr = Graphics.FromImage(bmp);
                //gr.DrawRectangle(pen, rect);
                //gr.DrawLine(new Pen(Color.YellowGreen), new Point(0, bmp.Height / 2), new Point(bmp.Width, bmp.Height / 2));
                //pictureBox1.Image = bmp;
            }
        }
    }
}

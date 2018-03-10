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

namespace OCRLib
{
    public partial class Form1 : Form
    {
        public Form1()
        {
            InitializeComponent();
        }

        public static Bitmap GetBlackAndWhiteImage(Bitmap SourceImage)
        {

            Bitmap bmp = new Bitmap(SourceImage.Width, SourceImage.Height);

            using (Graphics gr = Graphics.FromImage(bmp)) // SourceImage is a Bitmap object
            {
                var gray_matrix = new float[][] {
                    new float[] { 0.299f, 0.299f, 0.299f, 0, 0 },
                    new float[] { 0.587f, 0.587f, 0.587f, 0, 0 },
                    new float[] { 0.114f, 0.114f, 0.114f, 0, 0 },
                    new float[] { 0,      0,      0,      1, 0 },
                    new float[] { 0,      0,      0,      0, 1 }
            };

                var ia = new System.Drawing.Imaging.ImageAttributes();
                ia.SetColorMatrix(new System.Drawing.Imaging.ColorMatrix(gray_matrix));
                ia.SetThreshold(0.7f); // Change this threshold as needed
                var rc = new Rectangle(0, 0, SourceImage.Width, SourceImage.Height);
                gr.DrawImage(SourceImage, rc, 0, 0, SourceImage.Width, SourceImage.Height, GraphicsUnit.Pixel, ia);
            }
            return bmp;
        }

        Bitmap m_bw = null;
        Bitmap bmp_menu = new Bitmap(@"F:\menu.bmp");
        List<CharRow> chars_row = new List<CharRow>();
        Pen pen = new Pen(new SolidBrush(Color.Red));
        Pen pen_green = new Pen(new SolidBrush(Color.Green));

        private void Form1_Load(object sender, EventArgs e)
        {
            m_bw = GetBlackAndWhiteImage(bmp_menu);

                //bmp_menu.Clone(new Rectangle(0, 0, bmp_menu.Width, bmp_menu.Height),
                //PixelFormat.Format1bppIndexed);

            int row_start_end = 0;

            int line_start = 0;
            for (int y = 0; y < bmp_menu.Height; y++)
            {
                bool bLine = true;

                for (int x = 0; x < bmp_menu.Width; x++)
                {
                    Color clr = m_bw.GetPixel(x, y);
                    if (clr != Color.FromArgb(255, Color.White))
                    {
                        bLine = false;
                        break;
                    }
                }


                if (bLine)
                {
                    if (y == 0)
                    {// first row is line so search for begin
                        row_start_end++;
                        continue;
                    }

                    if (row_start_end % 2 == 0)
                    {
                        row_start_end++;
                        //Add Image to rows
                        Rectangle cloneRect = Rectangle.FromLTRB(0, line_start, bmp_menu.Width, y);
                        chars_row.Add(new CharRow(cloneRect));
                    }
                }
                else
                {
                    if (row_start_end % 2 != 0)
                    {
                        line_start = y;

                        row_start_end++;
                    }
                }
            }

            pen.DashStyle = System.Drawing.Drawing2D.DashStyle.Dot;
            pen_green.DashStyle = System.Drawing.Drawing2D.DashStyle.Dot;

            Graphics gr = Graphics.FromImage(bmp_menu);

            row_start_end = 0;
            line_start = 0;
            foreach (CharRow row in chars_row)
            {
                Rectangle rect = row.m_rect;
                for (int x = rect.Left; x < rect.Right; x++)
                {
                    bool bRow = true;

                    for (int y = rect.Top; y < rect.Bottom; y++)
                    {
                        Color clr = m_bw.GetPixel(x, y);
                        if (clr != Color.FromArgb(255, Color.White))
                        {
                            bRow = false;
                            break;
                        }
                    }
                    if (bRow)
                    {
                        if (x == 0)
                        {// first row is line so search for begin
                            row_start_end++;
                            continue;
                        }

                        if (row_start_end % 2 == 0)
                        {
                            row_start_end++;
                            Rectangle cloneRect = Rectangle.FromLTRB(line_start, rect.Top, x, rect.Bottom);
                            //gr.DrawRectangle(pen_green, cloneRect);
                            row.Add(new CharRect(cloneRect));
                        }
                    }
                    else
                    {
                        if (row_start_end % 2 != 0)
                        {
                            line_start = x;
                            row_start_end++;
                        }
                    }
                }
            }

            int middle_w = 0;
            int middle_h = 0;
            int nChars = 0;
            foreach (CharRow row in chars_row)
            {
                for (int i = 0; i < row.Count; i++ )
                {
                    Rectangle rect = row[i].m_rect;
                    Point pt1 = Point.Empty;
                    Point pt2 = Point.Empty;
                    for (int y = rect.Top; y < rect.Bottom; y++)
                    {
                        for (int x = rect.Left; x < rect.Right; x++)
                        {
                            Color clr = m_bw.GetPixel(x, y);
                            if (clr != Color.FromArgb(255, Color.White))
                            {
                                if (pt2 == Point.Empty)
                                {
                                    pt2 = new Point(x, y);
                                    pt1 = new Point(x, y);
                                }

                                pt1.X = Math.Min(x, pt1.X);
                                pt2.X = Math.Max(x, pt2.X);

                                pt1.Y = Math.Min(y, pt1.Y);
                                pt2.Y = Math.Max(y, pt2.Y);
                            }
                        }
                    }

                    middle_w += row[i].m_rect.Width;
                    middle_h += row[i].m_rect.Height;
                    nChars++;

                    //if (pt1.X == pt2.X)
                    {
                        if(pt1.X > 0) pt1.X--;
                        if (pt1.X < m_bw.Width) pt2.X++; 
                    }
                    //if (pt1.Y == pt2.Y)
                    {
                        if (pt1.Y > 0) pt1.Y--;
                        if (pt1.X < m_bw.Height) pt2.Y++;
                    }

                    row[i].m_rect = Rectangle.FromLTRB(pt1.X, pt1.Y, pt2.X, pt2.Y);
                    Rectangle temp = Rectangle.FromLTRB(pt1.X, pt1.Y, pt2.X, pt2.Y);
                    gr.DrawRectangle(pen, temp);
                }
            }
            
            gr.Flush();
            pictureBox1.Image = bmp_menu;
            middle_w = middle_w / nChars;
            middle_h = middle_h / nChars;
            //Recognize();
        }

        public Font GetAdjustedFont(Graphics GraphicRef, string GraphicString, Font OriginalFont, int ContainerWidth, int MaxFontSize, int MinFontSize, bool SmallestOnFail)
        {
            // We utilize MeasureString which we get via a control instance           
            for (int AdjustedSize = MaxFontSize; AdjustedSize >= MinFontSize; AdjustedSize--)
            {
                Font TestFont = new Font(OriginalFont.Name, AdjustedSize, OriginalFont.Style);

                // Test the string with the new size
                SizeF AdjustedSizeNew = GraphicRef.MeasureString(GraphicString, TestFont);

                if (ContainerWidth > Convert.ToInt32(AdjustedSizeNew.Width))
                {
                    // Good font, return it
                    return TestFont;
                }
            }

            // If you get here there was no fontsize that worked
            // return MinimumSize or Original?
            if (SmallestOnFail)
            {
                return new Font(OriginalFont.Name, MinFontSize, OriginalFont.Style);
            }
            else
            {
                return OriginalFont;
            }
        }

        Bitmap CreatePrintTemplate(int w, string strPrint)
        {
            Bitmap printBmp = null;
            try
            {
                printBmp = new Bitmap(w*2, w*2);
                RectangleF rectf = new RectangleF(0, 0, printBmp.Width, printBmp.Height);

                Graphics g = Graphics.FromImage(printBmp);
                g.FillRectangle(new SolidBrush(Color.White), rectf);
                Font font = new Font(SystemFonts.MenuFont.FontFamily, 10);

                font = GetAdjustedFont(g, strPrint, font, w, 12, 6, true);
                g = Graphics.FromImage(printBmp);

                // The smoothing mode specifies whether lines, curves, and the edges of filled areas use smoothing (also called antialiasing). One exception is that path gradient brushes do not obey the smoothing mode. Areas filled using a PathGradientBrush are rendered the same way (aliased) regardless of the SmoothingMode property.
                //g.SmoothingMode = SmoothingMode.AntiAlias;

                // The interpolation mode determines how intermediate values between two endpoints are calculated.
                g.InterpolationMode = InterpolationMode.HighQualityBicubic;

                // Use this property to specify either higher quality, slower rendering, or lower quality, faster rendering of the contents of this Graphics object.
                g.PixelOffsetMode = PixelOffsetMode.HighQuality;

                // This one is important
                g.TextRenderingHint = TextRenderingHint.AntiAliasGridFit;

                // Create string formatting options (used for alignment)
                StringFormat format = new StringFormat()
                {
                    Alignment = StringAlignment.Center,
                    LineAlignment = StringAlignment.Center
                };

                // Draw the text onto the image
                g.DrawString(strPrint, font, Brushes.Black, rectf, format);

                // Flush all graphics changes to the bitmap
                g.Flush();
            }
            catch (Exception ex)
            {
                return null;
            }

            return GetBlackAndWhiteImage(printBmp);
        }

        Bitmap GetLetterRect(string str)
        {
            Bitmap bmp = CreatePrintTemplate(10, str);
            Rectangle rect = new Rectangle(0,0,bmp.Width, bmp.Height);
            Point pt1 = Point.Empty;
            Point pt2 = Point.Empty;
            for (int y = rect.Top; y < rect.Bottom; y++)
            {
                for (int x = rect.Left; x < rect.Right; x++)
                {
                    Color clr = bmp.GetPixel(x, y);
                    if (clr != Color.FromArgb(255, Color.White))
                    {
                        if (pt2 == Point.Empty)
                        {
                            pt2 = new Point(x, y);
                            pt1 = new Point(x, y);
                        }

                        pt1.X = Math.Min(x, pt1.X);
                        pt2.X = Math.Max(x, pt2.X);

                        pt1.Y = Math.Min(y, pt1.Y);
                        pt2.Y = Math.Max(y, pt2.Y);
                    }
                    else 
                    {
                        int g = 0;
                    }
                }
            }


            //if (pt1.X == pt2.X)
            {
                pt1.X--;
                pt2.X++;
            }
            //if (pt1.Y == pt2.Y)
            {
                pt1.Y--;
                pt2.Y++;
            }
            Rectangle temp = Rectangle.FromLTRB(pt1.X, pt1.Y, pt2.X, pt2.Y);

            return bmp.Clone(temp, bmp.PixelFormat);
        }

        void Recognize(Point pt)
        {
            Graphics gr = Graphics.FromImage(bmp_menu);

            Recognizer recogn = new Recognizer();
            Bitmap bmp_templ = GetLetterRect("P");
            //Graphics g = Graphics.FromImage(bmp);
            //g.DrawRectangle(pen_green, 0, 0, bmp.Width, bmp.Height);

            var bmp_orig = new Bitmap(bmp_templ);
            var graph = Graphics.FromImage(bmp_orig);
            SolidBrush brush = new SolidBrush(Color.White);

            for (int j = 0; j < chars_row.Count; j++ )
            {
                CharRow row = chars_row[j];
                for (int i = 0; i < row.Count; i++)
                {
                    Rectangle rect = row[i].m_rect;
                    //if (rect.Width <= 3 || rect.Height <= 3)
                    {
                        //continue;
                    }
                    if (pt != Point.Empty && !rect.Contains(pt))
                    {
                        continue;
                    }

                    graph.FillRectangle(brush, new RectangleF(0, 0, bmp_orig.Width, bmp_orig.Height));
                    graph.DrawImage(m_bw, new Rectangle(0, 0, bmp_orig.Width, bmp_orig.Height),
                        rect, GraphicsUnit.Pixel);
                    float result = recogn.Compare(bmp_templ, bmp_orig);
                    //if (result > 0.5)
                    {
                        gr.DrawRectangle(pen, rect);
                        pictureBox2.Image = bmp_orig;
                        pictureBox3.Image = bmp_templ;
                        //System.Threading.Thread.Sleep(1000);
                        listBox1.Items.Clear();
                        listBox1.Items.Add(result.ToString());
                        return;
                    }
                }
            }
        }


        Point GetClick()
        {
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
            Recognize(clck);
        }
    }
}

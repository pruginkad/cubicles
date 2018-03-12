using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Drawing.Text;

namespace OCRLib
{
    public class Recognizer
    {
        public float Compare(Bitmap template, Bitmap image)
        {
            float overlap = 0;
            float total = template.Width*template.Height;
            for (int x = 0; x < template.Width; x++)
            {
                for (int y = 0; y < template.Height; y++)
                {
                    Color clr1 = template.GetPixel(x, y);
                    Color clr2 = image.GetPixel(x, y);
                    if (clr1 == clr2)
                    {
                        overlap += 1;                        
                    }
                    else
                    {
                        //image.SetPixel(x, y, Color.Green);
                    }
                }
            }
            float ret = overlap / total;
            return ret;
        }
        public static Bitmap GetBlackAndWhiteImage(Bitmap SourceImage)
        {
            Bitmap bmp = new Bitmap(SourceImage.Width, SourceImage.Height);

            using (Graphics gr = Graphics.FromImage(bmp)) // SourceImage is a Bitmap object
            {
                var gray_matrix = new float[][] 
                {
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

        public Bitmap m_bw = null;
        public Rectangle m_cut_menu_rect = Rectangle.Empty;

        public List<CharRow> chars_row = new List<CharRow>();

        public Color m_letterBkColor = Color.FromArgb(255, Color.White);
        public void LoadBmp(Bitmap bmp_menu)
        {
            m_cut_menu_rect = new Rectangle(0, 0, bmp_menu.Width, bmp_menu.Height);
            m_cut_menu_rect = CutMenuItem(bmp_menu);

            chars_row.Clear();
            m_bw = GetBlackAndWhiteImage(bmp_menu);

            int row_start_end = 0;

            int line_start = 0;
            for (int y = m_cut_menu_rect.Top; y < m_cut_menu_rect.Bottom; y++)
            {
                bool bLine = true;

                for (int x = m_cut_menu_rect.Left; x < m_cut_menu_rect.Right; x++)
                {
                    Color clr = m_bw.GetPixel(x, y);

                    if (clr != m_letterBkColor)
                    {
                        bLine = false;
                        break;
                    }
                }


                if (bLine)
                {
                    if (y == m_cut_menu_rect.Top)
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
                        if (clr != m_letterBkColor)
                        {
                            bRow = false;
                            break;
                        }
                    }
                    if (bRow)
                    {
                        if (x == m_cut_menu_rect.Left)
                        {// first row is line so search for begin
                            row_start_end++;
                            continue;
                        }

                        if (row_start_end % 2 == 0)
                        {
                            row_start_end++;
                            Rectangle cloneRect = Rectangle.FromLTRB(line_start, rect.Top, x, rect.Bottom);
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

            foreach (CharRow row in chars_row)
            {
                for (int i = 0; i < row.Count; i++)
                {
                    Rectangle rect = row[i].m_rect;
                    Point pt1 = Point.Empty;
                    Point pt2 = Point.Empty;
                    for (int y = rect.Top; y < rect.Bottom; y++)
                    {
                        for (int x = rect.Left; x < rect.Right; x++)
                        {
                            Color clr = m_bw.GetPixel(x, y);
                            if (clr != m_letterBkColor)
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

                    if (pt1.X > 0) pt1.X--;
                    if (pt1.X < m_bw.Width) pt2.X++;


                    if (pt1.Y > 0) pt1.Y--;
                    if (pt1.X < m_bw.Height) pt2.Y++;

                    row[i].m_rect = Rectangle.FromLTRB(pt1.X, pt1.Y, pt2.X, pt2.Y);
                    Rectangle temp = Rectangle.FromLTRB(pt1.X, pt1.Y, pt2.X, pt2.Y);
                }
            }
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

        Bitmap CreateLetterTemplate(int w, string strPrint)
        {
            Bitmap printBmp = null;
            try
            {
                printBmp = new Bitmap(w * 2, w * 2);
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
            Bitmap bmp = CreateLetterTemplate(10, str);
            Rectangle rect = new Rectangle(0, 0, bmp.Width, bmp.Height);
            Point pt1 = Point.Empty;
            Point pt2 = Point.Empty;
            for (int y = rect.Top; y < rect.Bottom; y++)
            {
                for (int x = rect.Left; x < rect.Right; x++)
                {
                    Color clr = bmp.GetPixel(x, y);
                    if (clr != m_letterBkColor)
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


            if (pt1.X > 0) pt1.X--;
            if (pt1.X < bmp.Width) pt2.X++;


            if (pt1.Y > 0) pt1.Y--;
            if (pt1.X < bmp.Height) pt2.Y++;

            Rectangle temp = Rectangle.FromLTRB(pt1.X, pt1.Y, pt2.X, pt2.Y);

            return bmp.Clone(temp, bmp.PixelFormat);
        }

        SolidBrush white_brush = new SolidBrush(Color.White);

        Bitmap GetBWRect(Rectangle src_rect, Rectangle dst_rect)
        {
            Bitmap bmp_orig = new Bitmap(dst_rect.Width, dst_rect.Height);
            using (Graphics graph = Graphics.FromImage(bmp_orig))
            {
                graph.FillRectangle(white_brush, new RectangleF(0, 0, bmp_orig.Width, bmp_orig.Height));
                graph.DrawImage(m_bw, dst_rect,
                    src_rect.Left, src_rect.Top, src_rect.Width, src_rect.Height, GraphicsUnit.Pixel);
                graph.Flush();
            }
            return GetBlackAndWhiteImage(bmp_orig);
        }

        public CharRow Recognize(string the_word, List<string> arrAddLetters)
        {
            //Graphics gr = Graphics.FromImage(bmp_menu);
            //listBox1.Items.Clear();

            List<string> arrLetters = the_word.Select(x => new string(x, 1)).ToList();
            arrLetters.AddRange(arrAddLetters);
            
            List<Bitmap> list_templ = new List<Bitmap>();
            for (int i = 0; i < arrLetters.Count; i++)
            {
                list_templ.Add(GetLetterRect(arrLetters[i]));
            }

            for (int j = 0; j < chars_row.Count; j++)
            {
                string cur_word = string.Empty;
                CharRow row = chars_row[j];

                for (int i = 0; i < row.Count; i++)
                {
                    float cur_res = 0;
                    string cur_letter = string.Empty;

                    CharRect char_rect = row[i];
                    char_rect.m_weights.Clear();

                    for (int l = 0; l < list_templ.Count; l++)
                    {
                        Bitmap bmp_templ = list_templ[l];


                        Rectangle rect = char_rect.m_rect;
                        if (rect.Width <= 1 || rect.Height <= 1)
                        {
                            continue;
                        }
                        //if (pt != Point.Empty
                        //    && !rect.Contains(pt)
                        //    )
                        //{
                        //    continue;
                        //}

                        Bitmap bmp_orig = GetBWRect(rect, new Rectangle(0, 0, bmp_templ.Width, bmp_templ.Height));


                        float result = Compare(bmp_templ, bmp_orig);
                        char_rect.m_weights.Add(arrLetters[l], result);
                        if (cur_res < result)
                        {
                            cur_res = result;
                            cur_letter = arrLetters[l];
                        }
                        
                    }
                    cur_word += cur_letter;
                }
                if (cur_word.ToLower().Contains(the_word.ToLower()))
                {
                    return row;
                }
            }
            return null;
        }
        
        public CharRow RecognizeByPoint(Point pt, string the_word, out Bitmap templ, out Bitmap orig)
        {
            //Graphics gr = Graphics.FromImage(bmp_menu);
            //listBox1.Items.Clear();

            string[] arrLetters = the_word.Select(x => new string(x, 1)).ToArray();


            Bitmap bmp_templ = GetLetterRect(the_word);

            templ = bmp_templ;

            for (int j = 0; j < chars_row.Count; j++)
            {
                CharRow row = chars_row[j];

                for (int i = 0; i < row.Count; i++)
                {
                    string cur_letter = string.Empty;

                    CharRect char_rect = row[i];
                    char_rect.m_weights.Clear();


                    Rectangle rect = char_rect.m_rect;
                    if (rect.Width <= 1 || rect.Height <= 1)
                    {
                        continue;
                    }
                    if (pt != Point.Empty
                        && !rect.Contains(pt)
                        )
                    {
                        continue;
                    }

                    Bitmap bmp_orig = GetBWRect(rect, new Rectangle(0, 0, bmp_templ.Width, bmp_templ.Height));
                    orig = bmp_orig;

                    float result = Compare(bmp_templ, bmp_orig);
                    return row;
                }

            }
            orig = null;
            return null;
        }
        
        public Rectangle CutMenuItem(Bitmap bmp)
        {
            float brightness_white = (float)0.0;
            //Cut Selection
            for (int y = 0; y < bmp.Height; y++)
            {
                int x = bmp.Width / 2;
                Color clr = bmp.GetPixel(x, y);
                //if (clr.GetBrightness() > brightness_white)
                {
                    brightness_white += clr.GetBrightness();
                }
            }

            brightness_white = brightness_white / bmp.Height;
            
            //brightness_white = brightness_white - (float)0.01;

            int top = 0;
            int bottom = bmp.Height - 1;
            for (int y = bmp.Height / 2; y >= 0; y--)
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
            if (rect.Height < 2)
            {
                rect = new Rectangle(0, 0, bmp.Width, bmp.Height);
            }
            return rect;
        }
    }

}

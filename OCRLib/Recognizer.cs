using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Drawing;

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
                        if (clr2 != Color.FromArgb(255, Color.White) &&
                            clr2 != Color.FromArgb(255, Color.Black))
                        {
                            int g = 0;
                        }
                        //if (clr2 != Color.FromArgb(255, Color.White))
                        {
                            overlap += 1;
                        }
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
    }
}

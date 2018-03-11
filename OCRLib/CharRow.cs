using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Drawing;

namespace OCRLib
{
    public class CharRect
    {
        public CharRect(Rectangle rect)
        {
            m_rect = rect;
        }
        public Rectangle m_rect
        {
            get;
            set;
        }

        public Dictionary<string, float> m_weights = new Dictionary<string, float>();
    }

    public class CharRow : List<CharRect>
    {
        public CharRow(Rectangle lines_bmp)
        {
            m_rect = lines_bmp;
            m_FullRect = lines_bmp;
        }
        public Rectangle m_rect = new Rectangle();
        
        public Rectangle m_FullRect
        {
            get;
            set;
        }
    }
}

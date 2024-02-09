using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Media;

namespace Painting
{
    internal class Tool
    {
        Brush color;
        int offset;

        public int width, height;
        public Point pos;
        public Point vector;

        public Tool(double x, double y)
        {
            width = 30;
            height = 30;
            offset = width / 2;

            pos = new Point(x - offset, y - offset);
            color = Brushes.Red;

            vector = new Point(x, y);
        }

        public void Move()
        {
            pos.X += vector.X;
            pos.Y += vector.Y;
        }

        public void Draw(DrawingContext dc)
        {
            Rect rect = new Rect()
            {
                X = pos.X,
                Y = pos.Y,
                Width = width,
                Height = height
            };
            
            dc.DrawRectangle(color, null, rect);
        }
    }
}

using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Windows.Forms;
using System.Drawing.Imaging;
using System.Windows.Media.Imaging;
using System.IO;

namespace FLib
{
    /// <summary>
    /// BitmapIteratorとは別。注意
    /// </summary>
    static public class BitmapHandler
    {
        static public BitmapIterator GetBitmapIterator(Bitmap bmp, ImageLockMode lockMode, PixelFormat pixelFormat)
        {
            return new BitmapIterator(bmp, lockMode, pixelFormat);
        }

        static public Bitmap CreateThumbnail(Bitmap bmp, int w, int h)
        {
            return CreateThumbnail(bmp, w, h, Color.White);
        }

        static public Bitmap CreateThumbnail(Bitmap bmp, int w, int h, Color bgColor)
        {
            Bitmap thumbnail = new Bitmap(w, h, bmp.PixelFormat);
            using (Graphics g = Graphics.FromImage(thumbnail))
            {
                g.Clear(bgColor);
                float ratio = Math.Min((float)w / bmp.Width, (float)h / bmp.Height);
                g.DrawImage(bmp, new Rectangle(0, 0, (int)(bmp.Width * ratio), (int)(bmp.Height * ratio)));
            }
            return thumbnail;
        }

        static public Bitmap FromSketch(List<List<Point>> sketch, int w, int h, Pen pen, Color clearColor)
        {
            Bitmap bmp = new Bitmap(w, h);
            using (Graphics g = Graphics.FromImage(bmp))
            {
                g.Clear(clearColor);
                foreach (var stroke in sketch)
                {
                    if (stroke.Count >= 2)
                    {
                        g.DrawLines(pen, stroke.ToArray());
                    }
                }
            }
            return bmp;
        }

        static public Bitmap FromSketchFile(string filePath, int w, int h, Pen pen, Color clearColor)
        {
            if (!System.IO.File.Exists(filePath)) return null;

            string[] lines = System.IO.File.ReadAllLines(filePath);
            List<List<Point>> sketch = new List<List<Point>>();
            foreach (var line in lines)
            {
                List<Point> stroke = new List<Point>();
                string[] pts = line.Split(' ').Where(str => string.IsNullOrWhiteSpace(str) == false).ToArray();
                foreach (var ptText in pts)
                {
                    string[] tokens = ptText.Split(',');
                    System.Diagnostics.Debug.Assert(tokens.Length == 2);
                    int x, y;
                    if (int.TryParse(tokens[0], out x) && int.TryParse(tokens[1], out y))
                    {
                        stroke.Add(new Point(x, y));
                    }
                }
                sketch.Add(stroke);
            }

            return FromSketch(sketch, w, h, pen, clearColor);
        }

        public static BitmapSource CreateBitmapSource(Bitmap bitmap)
        {
            if (bitmap == null)
                throw new ArgumentNullException("bitmap");

            return System.Windows.Interop.Imaging.CreateBitmapSourceFromHBitmap(
                bitmap.GetHbitmap(),
                IntPtr.Zero,
                System.Windows.Int32Rect.Empty,
                BitmapSizeOptions.FromEmptyOptions());
        }
    }
}
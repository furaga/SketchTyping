using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Windows.Forms;
using System.Drawing.Imaging;
using System.IO;


namespace FLib
{
    public enum StrokeType { Line, Ellipse, None };
    public class BeautificatedStroke
    {
        public StrokeType type;
        public List<Point> orgStroke;
        public List<Point> Stroke;

        public BeautificatedStroke(List<Point> stroke)
        {
            type = StrokeType.None;
            orgStroke = stroke;
            stroke = new List<Point>(stroke);

            if (stroke.Count <= 1) return;

            float maxSqDist = 0;
            foreach (var pt in stroke)
            {
                float dx = stroke[0].X - pt.X;
                float dy = stroke[0].Y - pt.Y;
                float dist = dx * dx + dy * dy;
                maxSqDist = Math.Max(maxSqDist, dist);
            }

            float edgeDX = stroke[0].X - stroke.Last().X;
            float edgeDY = stroke[0].Y - stroke.Last().Y;
            float edgeSqDist = edgeDX * edgeDX + edgeDY * edgeDY;

            float ratio = edgeSqDist / maxSqDist;

            if (ratio <= 0.1)
            {
                type = StrokeType.Ellipse;
            }
            else// if (ratio >= 0.9)
            {
                type = StrokeType.Line;
                Stroke = new List<Point>();
                Stroke.Add(stroke.First());
                Stroke.Add(stroke.Last());
            }
        }
    }

    /// <summary>
    /// アドホックにフリーストロークのスケッチを直線・円の集合として近似する
    /// SketchTypingによる画像検索に使う
    /// </summary>
    public class BeautificatedSketch
    {
        public int Count { get { return sketch.Count; } }

        public List<BeautificatedStroke> sketch = new List<BeautificatedStroke>();

        public BeautificatedSketch(List<List<Point>> rawSketch)
        {
            foreach (var stroke in rawSketch)
            {
                sketch.Add(new BeautificatedStroke(stroke));
            }
        }
        public BeautificatedSketch()
        {

        }

        public BeautificatedSketch(string filePath)
        {
            if (!System.IO.File.Exists(filePath)) return;

            string[] lines = System.IO.File.ReadAllLines(filePath);
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
                if (stroke.Count <= 0) continue;
                sketch.Add(new BeautificatedStroke(stroke));
            }
        }

        public Bitmap ToBitmap(int w, int h, Color clearColor, int penWidth)
        {
            Bitmap bmp = new Bitmap(w, h);
            using (Graphics g = Graphics.FromImage(bmp))
            {
                g.Clear(clearColor);
                foreach (var stroke in sketch)
                {
                    if (stroke.orgStroke.Count >= 2)
                    {
                        g.DrawLines(new Pen(
                            (stroke.type == StrokeType.None ? Brushes.Black :
                            stroke.type == StrokeType.Line ? Brushes.Red : Brushes.Blue), penWidth),
                            stroke.orgStroke.ToArray());
                    }
                }
            }
            return bmp;
        }

        public void Reflesh()
        {
            for (int i = sketch.Count - 1; i >= 0; i--)
            {
                if (sketch[i].Stroke == null || sketch[i].Stroke.Count <= 1)
                {
                    sketch.RemoveAt(i);
                }
            }
        }
    }
}

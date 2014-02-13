using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Windows.Forms;
using System.Diagnostics;

namespace FLib
{
    public class SketchTyping
    {
        Bitmap keyboardImage;
        List<Point> keyPoints = new List<Point>();
        public Dictionary<char, Point> keyPointsDict = new Dictionary<char, Point>();

        string keychars =
            @"????????????????" +
            @"?1234567890-^??" +
            @"?qwertyuiop@[?" +
            @"?asdfghjkl;:]" +
            @"?zxcvbnm,./??" +
            @"????? ???????";

        unsafe public SketchTyping(Bitmap keyboardImage)
        {
            this.keyboardImage = keyboardImage;
            List<Point> tmpKeyPoints = new List<Point>();
            using (BitmapIterator iter = new BitmapIterator(keyboardImage, System.Drawing.Imaging.ImageLockMode.ReadOnly, System.Drawing.Imaging.PixelFormat.Format32bppArgb))
            {
                for (int y = 0; y < keyboardImage.Height; y++)
                {
                    for (int x = 0; x < keyboardImage.Width; x++)
                    {
                        byte bb = iter.Data[4 * x + y * iter.Stride + 0];
                        byte gg = iter.Data[4 * x + y * iter.Stride + 1];
                        byte rr = iter.Data[4 * x + y * iter.Stride + 2];
                        if (rr == 255 && gg == 0 && bb == 0)
                        {
                            tmpKeyPoints.Add(new Point(x, y));
                        }
                    }
                }
            }

            const int sameRowThresDist = 30;

            while (tmpKeyPoints.Count >= 1)
            {
                Point offset = tmpKeyPoints.First();
                int removeCnt = 0;
                for (int i = 0; i < tmpKeyPoints.Count; i++)
                {
                    int dist = Math.Abs(offset.Y - tmpKeyPoints[i].Y);
                    if (dist < sameRowThresDist) removeCnt++;
                    else break;
                }
                var ls = tmpKeyPoints.Take(removeCnt).ToList();
                ls.Sort((pt1, pt2) => pt1.X - pt2.X);
                keyPoints.AddRange(ls);
                tmpKeyPoints.RemoveRange(0, removeCnt);
            }

            for (int i = 0; i < keyPoints.Count; i++)
            {
                keyPointsDict[keychars[i]] = keyPoints[i];
            }
        }

        public List<Point> GetStroke(string text)
        {
            text = text.Trim().ToLower();
            if (text.Length <= 0) return null;
            List<Point> stroke = new List<Point>();

            for (int i = 0; i < text.Length; i++)
            {
                if (keyPointsDict.ContainsKey(text[i]))
                {
                    stroke.Add(keyPointsDict[text[i]]);
                }
            }

            return stroke;
        }

        // ストロークの各セグメントのベクトルについてDPマッチング
        public float MinMatchingCost(List<Point> stroke1, List<Point> stroke2, RichTextBox richTextBox1 = null)
        {
            if (stroke1 == null || stroke2 == null) return float.MaxValue;

            float[] dp = new float[stroke1.Count * stroke2.Count];


            for (int i1 = 0; i1 < stroke1.Count; i1++) dp[i1] = i1;
            for (int i2 = 0; i2 < stroke2.Count; i2++) dp[i2 * stroke1.Count] = i2;

            for (int i2 = 1; i2 < stroke2.Count; i2++)
            {
                float vx2 = stroke2[i2].X - stroke2[i2 - 1].X;
                float vy2 = stroke2[i2].Y - stroke2[i2 - 1].Y;
                float sqLength2 = vx2 * vx2 + vy2 * vy2;

                for (int i1 = 1; i1 < stroke1.Count; i1++)
                {
                    float vx1 = stroke1[i1].X - stroke1[i1 - 1].X;
                    float vy1 = stroke1[i1].Y - stroke1[i1 - 1].Y;
                    float sqLength1 = vx1 * vx1 + vy1 * vy1;

                    float dx = vx1 - vx2;
                    float dy = vy1 - vy2;
                    float diff = (dx * dx + dy * dy) / Math.Max(sqLength1, sqLength2);

                    dp[i1 + i2 * stroke1.Count] = Math.Min(
                            dp[(i1 - 1) + i2 * stroke1.Count] + 1, Math.Min(
                            dp[i1 + (i2 - 1) * stroke1.Count] + 1,
                            dp[(i1 - 1) + (i2 - 1) * stroke1.Count] + diff));
                }
            }

            if (richTextBox1 != null)
            {
                string text = "";
                for (int i2 = 0; i2 < stroke2.Count; i2++)
                {
                    for (int i1 = 0; i1 < stroke1.Count; i1++)
                    {
                        text += string.Format("{0:0.00} ", dp[i1 + i2 * stroke1.Count]);
                    }
                    text += "\n";
                }

                text += "Mathing cost: " + dp[stroke1.Count * stroke2.Count - 1] + "\n\n";
                richTextBox1.Text = text;
            }

            return dp[stroke1.Count * stroke2.Count - 1];
        }

    }
}

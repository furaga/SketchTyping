using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Windows.Forms;
using System.Xml.Serialization;

namespace FLib
{
    public class SketchTypeCommand
    {
        public string Text;
        public Dictionary<string, List<List<Point>>> gestureList = new Dictionary<string, List<List<Point>>>();
        public Dictionary<string, Bitmap> gestureImages = new Dictionary<string, Bitmap>();
        Pen pen = new Pen(Brushes.Red, 3);

        public static void SaveCommands(string filepath, List<SketchTypeCommand> commands)
        {
            try
            {
                StringBuilder sb = new StringBuilder();
                sb.AppendLine(commands.Count + "");
                for (int i = 0; i < commands.Count; i++)
                {
                    sb.AppendLine(commands[i].Text.Replace("\n", "<<NEWLINE>>").Replace("\r", "<<NEWLINER>>"));
                    sb.AppendLine(commands[i].gestureList.Count + "");
                    foreach (var gesture in commands[i].gestureList)
                    {
                        sb.AppendLine(gesture.Key);
                        sb.AppendLine(gesture.Value.Count + "");
                        foreach (var stroke in gesture.Value)
                        {
                            sb.AppendLine(string.Join(" ", stroke.Select(pt => pt.X + "," + pt.Y).ToArray()));
                        }
                    }
                }

                System.IO.File.WriteAllText(filepath, sb.ToString());
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex + ":" + ex.StackTrace);
            }
        }

        public static List<SketchTypeCommand> LoadCommands(string filepath, int imgWidth, int imgHeight)
        {
            try
            {
                List<SketchTypeCommand> commands = new List<SketchTypeCommand>();
                if (System.IO.File.Exists(filepath) == false) return commands;
                System.IO.StringReader sr = new System.IO.StringReader(System.IO.File.ReadAllText(filepath));
                int cnt = int.Parse(sr.ReadLine().Trim());
                for (int i = 0; i < cnt; i++)
                {
                    string text = sr.ReadLine().Replace("<<NEWLINE>>", "\n").Replace("<<NEWLINER>>", "\r");
                    commands.Add(new SketchTypeCommand(text));
                    int gestureCnt = int.Parse(sr.ReadLine().Trim());
                    for (int j = 0; j < gestureCnt; j++)
                    {
                        string key = sr.ReadLine().Trim();
                        int strokeCnt = int.Parse(sr.ReadLine().Trim());
                        List<List<Point>> gesture = new List<List<Point>>();
                        for (int k = 0; k < strokeCnt; k++)
                        {
                            gesture.Add(sr.ReadLine().Trim().Split(' ').Select(t =>
                            {
                                var token = t.Split(',');
                                return new Point(int.Parse(token[0]), int.Parse(token[1]));
                            }).ToList());
                        }
                        commands.Last().AddNewGesture(key, gesture, imgWidth, imgHeight);
                    }
                }
                return commands;
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex + ":" + ex.StackTrace);
                return new List<SketchTypeCommand>();
            }
        }


        public SketchTypeCommand(string text)
        {
            Text = text;
        }

        public void AddNewGesture(string key, List<List<Point>> gesture, int width, int height)
        {
            int minx = int.MaxValue;
            int miny = int.MaxValue;
            int maxx = int.MinValue;
            int maxy = int.MinValue;

            foreach (var stroke in gesture)
            {
                for (int i = 0; i < stroke.Count; i++)
                {
                    minx = Math.Min(minx, stroke[i].X);
                    miny = Math.Min(miny, stroke[i].Y);
                    maxx = Math.Max(maxx, stroke[i].X);
                    maxy = Math.Max(maxy, stroke[i].Y);
                }
            }

            int ox = minx - 3;
            int oy = miny - 3;
            int w = maxx - minx + 6;
            int h = maxy - miny + 6;
            using (Bitmap bmp = new Bitmap(w, h))
            {
                using (var g = Graphics.FromImage(bmp))
                {
                    g.Clear(Color.White);
                    foreach (var stroke in gesture)
                    {
                        if (stroke.Count >= 2)
                        {
                            g.DrawLines(pen, stroke.Select(pt => new Point(pt.X - ox, pt.Y - oy)).ToArray());
                        }
                    }
                }
                gestureImages[key] = BitmapHandler.CreateThumbnail(bmp, width, height);
            }
            gestureList[key] = gesture;
        }

        public void RemoveGesture(string key)
        {
            gestureImages[key].Dispose();
            gestureImages.Remove(key);
            gestureList.Remove(key);
        }
    }

}

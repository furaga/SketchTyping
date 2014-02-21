using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Windows.Forms;
using FLib;

namespace SketchTypingSketchSearch
{
    public partial class Form1 : Form
    {
        const string host = "127.0.0.1";
        const int port = 8094;
        const string gesturePath = "../../../Resource/gestureImageSearch.txt";
        System.Diagnostics.Process server = null;
        SketchTypingClient client;
        Timer timer = new Timer();

        Dictionary<string, Bitmap> primitiveThumbnails = new Dictionary<string, Bitmap>();
        Dictionary<string, BeautificatedSketch> sketches = new Dictionary<string, BeautificatedSketch>();
        BeautificatedSketch gestureStrokes = new BeautificatedSketch();

        public Form1()
        {
            InitializeComponent();
        }

        private void Form1_Load(object sender, EventArgs e)
        {
            foreach (var filepath in System.IO.Directory.GetFiles("../../../Resource").Where(f => f.EndsWith(".png")))
            {
                using (var bmp = new Bitmap(filepath))
                {
                    string key = System.IO.Path.GetFileNameWithoutExtension(filepath);
                    Bitmap value =  BitmapHandler.CreateThumbnail(bmp, commandList.ImageSize.Width, commandList.ImageSize.Height);
                    primitiveThumbnails[key] = value;                       
                    commandList.Images.Add(key, value);
                }
            }
        }

        void timer_Tick(object sender, EventArgs e)
        {
            if (client == null) return;

            string text = client.ReadString();

            try
            {
                if (text != "")
                {
                    if (primitiveThumbnails.ContainsKey(text))
                    {
                        listView1.Items.Add(text, text);
                        gestureStrokes.sketch.Add(GestureStrokeFromString(text));
                        Sort(sketches, gestureStrokes);
                    }
                }
            }
            catch (Exception ex)
            {
                                MessageBox.Show(ex + ":" + ex.StackTrace);
            }
            client.SendReceivedSignal();
        }

        private void Sort(Dictionary<string, BeautificatedSketch> sketches, BeautificatedSketch gestureStrokes)
        {
            listView3.Items.Clear();
            var sortedList = sketches.OrderBy(s => MinMatchingCost(s.Value, gestureStrokes)).ToArray();
            foreach (var kv in sortedList)
            {
                listView3.Items.Add(kv.Key, kv.Key);
            }
        }

        // ストロークの各セグメントのベクトルについてDPマッチング
        public float MinMatchingCost(BeautificatedSketch sketch1, BeautificatedSketch sketch2)
        {
            if (sketch1 == null || sketch2 == null) return float.MaxValue;

            sketch1.Reflesh();
            sketch2.Reflesh();

            float[] dp = new float[sketch1.Count * sketch2.Count];


            for (int i1 = 0; i1 < sketch1.Count; i1++) dp[i1] = i1;
            for (int i2 = 0; i2 < sketch2.Count; i2++) dp[i2 * sketch1.Count] = i2;

            for (int i2 = 1; i2 < sketch2.Count; i2++)
            {
                BeautificatedStroke stroke2 = sketch2.sketch[i2];
                float dirX2 = 0, dirY2 = 0;
                if (stroke2.type == StrokeType.Line)
                {
                    float vx2 = stroke2.Stroke.First().X - stroke2.Stroke.Last().X;
                    float vy2 = stroke2.Stroke.First().Y - stroke2.Stroke.Last().Y;
                    float length2 = (float)Math.Sqrt(vx2 * vx2 + vy2 * vy2);
                    dirX2 = vx2 / length2;
                    dirY2 = vy2 / length2;
                }
                for (int i1 = 1; i1 < sketch1.Count; i1++)
                {
                    BeautificatedStroke stroke1 = sketch1.sketch[i1];
                    float diff = stroke1.type == stroke2.type ? 0 : 1000;
                    float dirX1, dirY1;
                    if (stroke1.type == StrokeType.Line)
                    {
                        float vx1 = stroke1.Stroke.First().X - stroke1.Stroke.Last().X;
                        float vy1 = stroke1.Stroke.First().Y - stroke1.Stroke.Last().Y;
                        float length1 = (float)Math.Sqrt(vx1 * vx1 + vy1 * vy1);
                        dirX1 = vx1 / length1;
                        dirY1 = vy1 / length1;

                        float dx = dirX2 - dirX1;
                        float dy = dirY2 - dirY1;
                        diff = (float)Math.Sqrt(dx * dx + dy * dy);
                    }

                    dp[i1 + i2 * sketch1.Count] = Math.Min(
                            dp[(i1 - 1) + i2 * sketch1.Count] + 1, Math.Min(
                            dp[i1 + (i2 - 1) * sketch1.Count] + 1,
                            dp[(i1 - 1) + (i2 - 1) * sketch1.Count] + diff));
                }
            }

            // for Debug
            string text = "-------------------------\n";
            for (int i2 = 0; i2 < sketch2.Count; i2++)
            {
                for (int i1 = 0; i1 < sketch1.Count; i1++)
                {
                    text += string.Format("{0:0.00} ", dp[i1 + i2 * sketch1.Count]);
                }
                text += "\n";
            }
            text += "Mathing cost: " + dp[sketch1.Count * sketch2.Count - 1] + "\n\n";
            Console.WriteLine(text);

            return dp[sketch1.Count * sketch2.Count - 1] / sketch1.Count;
        }


        /// <summary>
        /// TODO!
        /// </summary>
        /// <param name="text"></param>
        /// <returns></returns>
        private BeautificatedStroke GestureStrokeFromString(string text)
        {
            const int scale = 400;
            if (text.StartsWith("Line"))
            {
                double angle = int.Parse(text.Substring("Line".Length)) * Math.PI / 180.0;
                double cos = Math.Cos(angle);
                double sin = Math.Sin(angle);
                return new BeautificatedStroke(
                    new List<Point>()
                    {
                        Point.Empty,
                        new Point((int)(scale * cos), -(int)(scale * sin)),
                    });
            }
            if (text.StartsWith("Ellipse"))
            {
                var stroke = new List<Point>();
                const int sample = 5;
                for (int i = 0; i <= sample; i++)
                {
                    double angle = 2 * Math.PI * i / sample;
                    double cos = Math.Cos(angle);
                    double sin = Math.Sin(angle);
                    stroke.Add(new Point((int)(scale * cos), -(int)(scale * sin)));
                }
                return new BeautificatedStroke(stroke);
            }
            return null;
        }

        private void inputGesturesGToolStripMenuItem_Click(object sender, EventArgs e)
        {
            inputGesturesGToolStripMenuItem.Checked = !inputGesturesGToolStripMenuItem.Checked;

            if (inputGesturesGToolStripMenuItem.Checked)
            {
                CreateSketchTyping();
            }
            else
            {
                DisposeSketchTyping();
            }
        }

        private void Form1_FormClosed(object sender, FormClosedEventArgs e)
        {
            DisposeSketchTyping();
        }

        void CreateSketchTyping()
        {
            try
            {
                server = System.Diagnostics.Process.Start(System.IO.Path.GetFullPath(SketchTypingServer.serverPath), host + " " + port + " " + gesturePath);
                client = new SketchTypingClient(host, port);
                timer.Interval = 66;
                timer.Tick += new EventHandler(timer_Tick);
                timer.Start();
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex + ":" + ex.StackTrace);
            }
        }

        void DisposeSketchTyping()
        {
            timer.Stop();
            try
            {
                if (client != null)
                {
                    client.Dispose();
                    client = null;
                }
                if (server != null)
                {
                    server.Kill();
                    server = null;
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex + ":" + ex.StackTrace);
            }
        }

        private void openSkechFolderOToolStripMenuItem_Click(object sender, EventArgs e)
        {
            openFileDialog1.RestoreDirectory= true;
            openFileDialog1.Filter = "sketch file|*.txt";
            if (openFileDialog1.ShowDialog() == System.Windows.Forms.DialogResult.OK)
            {
                string[] sketchFiles = System.IO.Directory
                    .GetFiles(System.IO.Path.GetDirectoryName(openFileDialog1.FileName))
                    .Where(f => f.EndsWith(".txt")).ToArray();

                for (int i = 0; i < sketchFiles.Length; i++)
                {
                    BeautificatedSketch sketch = new BeautificatedSketch(sketchFiles[i]);
                    sketches[sketchFiles[i]] = sketch;
                    using (var bmp = sketch.ToBitmap(400, 300, Color.White, 6))
                    {
                        var thumbnail = BitmapHandler.CreateThumbnail(bmp, sketchList.ImageSize.Width, sketchList.ImageSize.Height);
                        sketchList.Images.Add(sketchFiles[i], thumbnail);
                        listView2.Items.Add(sketchFiles[i], sketchFiles[i]);
                    }
                }
            }
        }

        private void listView1_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.KeyCode == Keys.Back)
            {
                if (listView1.Items.Count >= 1)
                {
                    listView1.Items.RemoveAt(listView1.Items.Count - 1);
                    gestureStrokes.sketch.RemoveAt(gestureStrokes.Count - 1);
                }
            }
        }

        private void listView2_ItemSelectionChanged(object sender, ListViewItemSelectionChangedEventArgs e)
        {
            if (listView2.SelectedItems.Count >= 1)
            {
                string filepath = listView2.SelectedItems[0].Text;
                sketch = new BeautificatedSketch(filepath);
                if (sketchBmp != null) sketchBmp.Dispose();
                sketchBmp = sketch.ToBitmap(400, 300, Color.White, 6);
                pictureBox1.Invalidate();
            }
        }

        BeautificatedSketch sketch;
        Bitmap sketchBmp;

        private void pictureBox1_Paint(object sender, PaintEventArgs e)
        {
            if (sketchBmp != null)
            {
                e.Graphics.DrawImage(sketchBmp, new Rectangle(0, 0, pictureBox1.Width, pictureBox1.Height));
            }
        }
    }
}

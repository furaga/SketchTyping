using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Windows.Forms;
using FLib;

namespace SketchTyping
{
    public partial class GestureForm : Form
    {
        Form1 owner = null;
        Bitmap canvasImage;
        

        public GestureForm(Form1 owner)
        {
            InitializeComponent();
            this.owner = owner;
            owner.hooker.OnKeyHook = OnKeyHook;
            canvasImage = new Bitmap(owner.keyboardImage);
            timer.Interval = 300;
            timer.Tick += new EventHandler(timer_Tick);
        }

        void timer_Tick(object sender, EventArgs e)
        {
            Terminate();
            timer.Enabled = false;
        }

        private void canvas_Paint(object sender, PaintEventArgs e)
        {
            float ratio = (float)canvas.Width / canvasImage.Width;
            int w = (int)(ratio * canvasImage.Width);
            int h = (int)(ratio * canvasImage.Height);
            e.Graphics.DrawImage(canvasImage,
                new Rectangle(
                    0,
                    canvas.Height / 2 - h / 2,
                    w,
                    h));
        }

        string inputText = "";

        void DrawCanvas()
        {
            if (owner == null) return;
            List<Point> stroke = owner.sketchTyping.GetStroke(inputText);
            using (Graphics g = Graphics.FromImage(canvasImage))
            {
                g.DrawImage(owner.keyboardImage, Point.Empty);
                if (stroke != null)
                {
                    for (int i = 0; i < stroke.Count - 1; i++)
                    {
                        g.DrawLine(new Pen(Brushes.Red, 3), stroke[i], stroke[i + 1]);
                    }
                }
            }
            canvas.Invalidate();
        }

        Timer timer = new Timer();

        bool OnKeyHook(int code, WM wParam, KBDLLHOOKSTRUCT lParam, Hooker hooker)
        {
            switch (wParam)
            {
                case WM.KEYDOWN:
                case WM.SYSKEYDOWN:
                    try
                    {
                        if (owner.sketchTyping.keyPointsDict.ContainsKey(char.ToLower((char)lParam.vkCode)))
                        {
                            timer.Enabled = false;
                            inputText += (char)lParam.vkCode;
                            DrawCanvas();
                            timer.Enabled = true;
                        }

                        if ((char)lParam.vkCode == '\n' || (char)lParam.vkCode == '\r')
                        {
                            Terminate();
                        }
                        return true;
                    }
                    catch (Exception e)
                    {
                        Console.WriteLine(e);
                    }
                    break;
            }
            return false;
        }

        void Terminate()
        {
            owner.hooker.OnKeyHook = owner.OnKeyHook;

            var inputStroke = owner.sketchTyping.GetStroke(inputText);

            string text = "";

            var commands = owner.commands;
            Dictionary<SketchTypeCommand, float> comCosts = new Dictionary<SketchTypeCommand, float>();
            foreach (var com in commands)
            {
                Console.Write(com.Text + ": ");
                float total = 0;
                List<float> costs = new List<float>();
                int cnt = 0;
                if (com.gestureList.Count <= 0) continue;
                foreach (var ges in com.gestureList)
                {
                    if (ges.Value.Count >= 1)
                    {
                        cnt++;
                        var refStroke = ges.Value[0];
                        float cost = owner.sketchTyping.MinMatchingCost(inputStroke, refStroke);
                        Console.Write(cost + " ");
                        cost = cost * cost;
                        total += cost;
                        costs.Add(cost);
                    }
                }

                float mean = total / costs.Count;        // 平均
                float sum = 0;
                foreach (int i in costs)
                {
                    float d = i - mean;
                    sum += d * d;
                }
                float variance = sum / costs.Count;   // 分散
                float stddev = (float)Math.Sqrt(variance);    // 標準偏差

                float eval = mean / stddev;
                Console.Write("[" + eval + "]\n");
                comCosts[com] = eval;
            }

            string[] sorted = comCosts.OrderBy(kv => kv.Value).Select(kv => kv.Key.Text).ToArray();
            Console.WriteLine(string.Join("<", sorted));


            text = sorted.First();

            owner.inputText = text;
            owner.inputTimer.Enabled = true;
            Close();
        }
    }
}

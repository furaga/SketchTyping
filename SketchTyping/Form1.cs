using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Windows.Forms;
using System.Diagnostics;
using FLib;


namespace SketchTyping
{
    public partial class Form1 : Form
    {
        public FLib.SketchTyping sketchTyping;
        const int KeyPointerHalfSize = 5;
        Bitmap canvasImage;
        public Bitmap keyboardImage;
        Font font = new Font("Arial", 10);
        GestureForm gestureForm;
        public Timer inputTimer = new Timer();
        public string inputText = "";

        public Form1()
        {
            InitializeComponent();
        }

        public List<SketchTypeCommand> commands = new List<SketchTypeCommand>();

        unsafe private void Form1_Load(object sender, EventArgs e)
        {
            string[] gestureFiles = System.IO.Directory.GetFiles("../../../Resource/").Where(f => f.EndsWith(".txt") && f.Contains("esture")).ToArray();
            Debug.Assert(gestureFiles.Length >= 1);

            comboBox1.Items.AddRange(gestureFiles);
            comboBox1.SelectedIndex = 0;

            commands = SketchTypeCommand.LoadCommands(gestureFiles.First(), 64, 64);

            keyboardImage = new Bitmap("../../../Resource/keyboard.png");
            canvasImage = new Bitmap(keyboardImage);
            sketchTyping = new FLib.SketchTyping(keyboardImage);

            using (Graphics g = Graphics.FromImage(canvasImage))
            {
                g.DrawImage(keyboardImage, Point.Empty);
                for (int i = 0; i < sketchTyping.keyPointsDict.Count; i++)
                {
                    char ch = sketchTyping.keyPointsDict.Keys.ElementAt(i);
                    Point pt = sketchTyping.keyPointsDict.Values.ElementAt(i);
                    g.FillEllipse(Brushes.Blue, new Rectangle(
                            pt.X - KeyPointerHalfSize, pt.Y - KeyPointerHalfSize,
                            KeyPointerHalfSize * 2, KeyPointerHalfSize * 2));
                    g.DrawString("" + ch, font, Brushes.Red, pt);
                }
            }

            canvas.Invalidate();
            inputTimer.Tick += new EventHandler(inputTimer_Tick);
            gestureForm = new GestureForm(this);
        }

        void inputTimer_Tick(object sender, EventArgs e)
        {
            Clipboard.Clear();
            Clipboard.SetText(inputText);
            System.Threading.Thread.Sleep(100);
            SendKeys.Send("^v");
            inputTimer.Enabled = false;
        }

        private void canvas_Paint(object sender, PaintEventArgs e)
        {
            float ratio = (float)canvas.Width / canvasImage.Width;
            int w = (int)(ratio * canvasImage.Width);
            int h = (int)(ratio * canvasImage.Height);
            e.Graphics.DrawImage(canvasImage, new Rectangle(0, canvas.Height / 2 - h / 2, w, h));
        }

        private void canvas_Resize(object sender, EventArgs e)
        {
            canvas.Invalidate();
        }

        Stopwatch sw = new Stopwatch();
        Point prevPoint;

        private void textBox2_TextChanged(object sender, EventArgs e)
        {
            long ticks = sw.ElapsedMilliseconds;

            sw.Restart();

            string text = textBox2.Text.Trim().ToLower();
            if (text.Length <= 0) return;
            using (Graphics g = Graphics.FromImage(canvasImage))
            {
                if (text.Length <= 1)
                {
                    g.DrawImage(keyboardImage, Point.Empty);
                }
                //                    for (int i = 0; i < text.Length; i++)
                int i = text.Length - 1;
                {
                    if (sketchTyping.keyPointsDict.ContainsKey(text[i]))
                    {
                        Point pt = sketchTyping.keyPointsDict[text[i]];
                        g.FillEllipse(Brushes.Blue, new Rectangle(
                                pt.X - KeyPointerHalfSize, pt.Y - KeyPointerHalfSize,
                                KeyPointerHalfSize * 2, KeyPointerHalfSize * 2));
                        if (0 < ticks && ticks <= 300)
                        {
                            g.DrawLine(new Pen(Brushes.Red, 3), prevPoint, pt);
                        }
                        prevPoint = pt;
                    }
                }
            }

            richTextBox1.Text += ticks + ": " + text.Length + "\n";
            richTextBox1.Select(richTextBox1.Text.Length - 1, 1);
            richTextBox1.ScrollToCaret();

            canvas.Invalidate();
        }



        private void button1_Click(object sender, EventArgs e)
        {
            string text1 = strokeText1.Text;
            string text2 = strokeText2.Text;

            List<Point> stroke1 = sketchTyping.GetStroke(text1);
            List<Point> stroke2 = sketchTyping.GetStroke(text2);

            sketchTyping.MinMatchingCost(stroke1, stroke2, richTextBox1);
        }

        private void strokeText1_TextChanged(object sender, EventArgs e)
        {
            List<Point> stroke1 = sketchTyping.GetStroke(strokeText1.Text);
            List<Point> stroke2 = sketchTyping.GetStroke(strokeText2.Text);
            using (Graphics g = Graphics.FromImage(canvasImage))
            {
                g.DrawImage(keyboardImage, Point.Empty);
                if (stroke1 != null)
                {
                    for (int i = 0; i < stroke1.Count - 1; i++)
                    {
                        g.DrawLine(new Pen(Brushes.Red, 3), stroke1[i], stroke1[i + 1]);
                    }
                }
                if (stroke2 != null)
                {
                    for (int i = 0; i < stroke2.Count - 1; i++)
                    {
                        g.DrawLine(new Pen(Brushes.Blue, 3), stroke2[i], stroke2[i + 1]);
                    }
                }
            }

            canvas.Invalidate();
        }

       public Hooker hooker = new Hooker();

        private void button2_Click(object sender, EventArgs e)
        {
            hooker.OnKeyHook = OnKeyHook;
            hooker.Hook();
        }

        public bool OnKeyHook(int code, WM wParam, KBDLLHOOKSTRUCT lParam, Hooker hooker)
        {
            switch (wParam)
            {
                case WM.KEYDOWN:
                case WM.SYSKEYDOWN:
                    try
                    {
                        if (!gestureForm.Visible && ToGestureMode((uint)lParam.vkCode))
                        {
                            gestureForm = new GestureForm(this);
                            gestureForm.Show();
                            return true;
                        }
                    }
                    catch (Exception e)
                    {
                        Console.WriteLine(e);
                    }
                    break;
            }
            return false;
        }


        //---------------------------------------------------------------
        // フック処理をすべき枝か
        //---------------------------------------------------------------
        bool ToGestureMode(uint vkCode = 0)
        {
            if (!hooker.onCtrl || (vkCode != (uint)'X' && vkCode != (uint)'x'))
            if (!hooker.onCtrl || (vkCode != (uint)'X' && vkCode != (uint)'x'))
            {
                return false;
            }
            return true;
        }

        private void button3_Click(object sender, EventArgs e)
        {
            hooker.Unhook();
        }

        private void button4_Click(object sender, EventArgs e)
        {
            commands = SketchTypeCommand.LoadCommands(comboBox1.Text, 64, 64);
        }
    }
}
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Windows.Forms;
using System.Diagnostics;
using System.Net;
using System.Net.Sockets;
using FLib;

namespace SketchTypingServer
{
    public partial class Form1 : Form
    {
        public SketchTyping sketchTyping;
        const int KeyPointerHalfSize = 5;
        Bitmap canvasImage;
        public Bitmap keyboardImage;
        Font font = new Font("Arial", 10);
        public string inputText = "";
        public List<SketchTypeCommand> commands = new List<SketchTypeCommand>();
        public Hooker hooker = new Hooker();
        Timer timer = new Timer();
        FLib.SketchTypingServer server;

        public Form1()
        {
            InitializeComponent();
        }

        private void Form1_Load(object sender, EventArgs e)
        {
            // コマンド設定ファイルの列挙
            string[] gestureFiles = System.IO.Directory.GetFiles("../../../Resource/").Where(f => f.EndsWith(".txt")).ToArray();
            Debug.Assert(gestureFiles.Length >= 1);
            comboBox1.Items.AddRange(gestureFiles);
            comboBox1.SelectedIndex = 0;

            // コマンド読み込み
            commands = SketchTypeCommand.LoadCommands(gestureFiles.First(), 64, 64);

            // SketchTypingオブジェクトを生成
            keyboardImage = new Bitmap("../../../Resource/keyboard.png");
            sketchTyping = new FLib.SketchTyping(keyboardImage);

            // キャンバス初期化
            canvasImage = new Bitmap(keyboardImage);

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

            string[] args = System.Environment.GetCommandLineArgs();

            // プロセス通信
            server = new FLib.SketchTypingServer(textBox1);

            // コマンド読み込み
            string gesturePath = args[3];
            comboBox1.Text = gesturePath;
            commands = SketchTypeCommand.LoadCommands(gesturePath, 64, 64);
            textBox1.Text += commands.Count + "\r\n";

            // ジェスチャ認識の準備
            hooker.OnKeyHook = OnKeyHook;
            timer.Interval = 300;
            timer.Tick += new EventHandler(timer_Tick);
            hooker.Hook();
        }

        void timer_Tick(object sender, EventArgs e)
        {
            Terminate();
            timer.Enabled = false;
        }

        void DrawCanvas()
        {
            List<Point> stroke = sketchTyping.GetStroke(inputText);
            using (Graphics g = Graphics.FromImage(canvasImage))
            {
                g.DrawImage(keyboardImage, Point.Empty);
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
        
        void Terminate()
        {
            float minCost;
            string commandName = sketchTyping.GetMatchingCommand(inputText, commands, out minCost, textBox1);
            if (minCost < threshold)
            {
                server.SendQuery(commandName);
            }
            Text = "[" + minCost + "]" + commandName;
            inputText = "";
        }

        private void canvas_Paint(object sender, PaintEventArgs e)
        {
            float ratio = (float)canvas.Width / canvasImage.Width;
            int w = (int)(ratio * canvasImage.Width);
            int h = (int)(ratio * canvasImage.Height);
            e.Graphics.DrawImage(canvasImage, new Rectangle(0, canvas.Height / 2 - h / 2, w, h));
        }

        bool OnKeyHook(int code, WM wParam, KBDLLHOOKSTRUCT lParam, Hooker hooker)
        {
            switch (wParam)
            {
                case WM.KEYDOWN:
                case WM.SYSKEYDOWN:
                    try
                    {
                        if (sketchTyping.keyPointsDict.ContainsKey(char.ToLower((char)lParam.vkCode)))
                        {
                            if (inputText.Length <= 0)
                            {
                                server.SendQuery("StartInput" + DateTime.Now.Ticks);
                            }
                            timer.Enabled = false;
                            inputText += (char)lParam.vkCode;
                            DrawCanvas();
                            timer.Enabled = true;
                        }
                        // タブボタン
                        if (lParam.vkCode == 0x09)
                        {
                            server.SendQuery("TabPressed");
                        }

                        return false;
                    }
                    catch (Exception e)
                    {
                        MessageBox.Show(e.ToString());
                    }
                    break;
            }
            return false;
        }

        private void button4_Click(object sender, EventArgs e)
        {
            commands = SketchTypeCommand.LoadCommands(comboBox1.Text, 64, 64);
        }

        private void hookingHToolStripMenuItem_Click(object sender, EventArgs e)
        {
            hookingHToolStripMenuItem.Checked = !hookingHToolStripMenuItem.Checked;
            if (hooker != null)
            {
                if (hookingHToolStripMenuItem.Checked)
                {
                    hooker.Hook();
                }
                else
                {
                    hooker.Unhook();
                }
            }
        }

        private void canvas_Resize(object sender, EventArgs e)
        {
            canvas.Invalidate();
        }

        float threshold = 0.55f;

        private void textBox2_TextChanged(object sender, EventArgs e)
        {
            double tmp;
            if (double.TryParse(textBox2.Text, out tmp))
            {
                threshold = (float)tmp;
            }
        }
    }
}

    using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Windows.Forms;
using FLib;


namespace SketchTypingDataCollect
{
    public partial class GestureForm : Form
    {
        Form1 owner = null;
        Bitmap canvasImage;
        Bitmap keyboardImage;
        Hooker hooker = new Hooker();
        public string inputText = "";
        public List<Point> stroke;
        SketchTyping sketchTyping;

        public GestureForm(Form1 owner)
        {
            InitializeComponent();
            this.owner = owner;
            hooker.OnKeyHook = OnKeyHook;
            keyboardImage = new Bitmap("keyboard.png");
            sketchTyping = new SketchTyping(keyboardImage);
            canvasImage = new Bitmap(keyboardImage);
        }

        public void HooAndShow()
        {
            inputText = "";
            stroke = null;
            hooker.Hook();
            DrawCanvas();
            ShowDialog();
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

        void DrawCanvas()
        {
            if (owner == null) return;
            stroke = sketchTyping.GetStroke(inputText);
            using (Graphics g = Graphics.FromImage(canvasImage))
            {
                g.DrawImage(keyboardImage, Point.Empty);
                if (stroke != null)
                {
                    if (stroke.Count >= 2)
                    {
                        g.DrawLines(new Pen(Brushes.Red, 3), stroke.ToArray());
                    }
                }
            }

            canvas.Invalidate();
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
                            inputText += (char)lParam.vkCode;
                            DrawCanvas();
                        }
                        if ((char)lParam.vkCode == '\n' || (char)lParam.vkCode == '\r')
                        {
                            hooker.Unhook();
                            Hide();
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

        private void GestureForm_Load(object sender, EventArgs e)
        {

        }
    }
}

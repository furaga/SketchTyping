using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Windows.Forms;
using System.IO;
using System.Diagnostics;
using FLib;

namespace Browser
{
    public partial class Form1 : Form
    {
        public Hooker hooker = new Hooker(); 
        public FLib.SketchTyping sketchTyping;
        public List<SketchTypeCommand> trainData = new List<SketchTypeCommand>();

        string gestureFile = "";
        Timer timer = new Timer();

        public Form1()
        {
            InitializeComponent();
        }

        private void Form1_Load(object sender, EventArgs e)
        {
            // init variables
            gestureFile = Path.GetFullPath("Gestures/gestureBrows.txt");
            threshold = GetThreshold();

            // load keyboard image
            if (!File.Exists("keyboard.png"))
            {
                MessageBox.Show("キーボード画像 keyboard.png が見つかりません");
                Application.Exit();
            }

            using (var bmp = new Bitmap("keyboard.png"))
                sketchTyping = new SketchTyping(new Bitmap(bmp));

            // set gesture combobox
            if (Directory.Exists("Gestures"))
                gestureComboBox.Items.AddRange(Directory.GetFiles("Gestures").Where(f => f.EndsWith(".txt")).ToArray());
            gestureComboBox.Text = gestureFile;

            // load train data
            if (!File.Exists(gestureFile))
            {
                MessageBox.Show("ジェスチャファイル " + gestureFile + " が見つかりません");
                Application.Exit();
            }
            trainData = SketchTypeCommand.LoadCommands(gestureFile, 64, 64);

            DebugLog("load train data: # = " + (trainData == null ? "null" : "" + trainData.Count));

            timer.Interval = 250;
            timer.Tick += new EventHandler(timer_Tick);

            UpdateGUI();
        }

        void DebugLog(string mes)
        {
            log.Text = mes;
            textBox.Text = mes + "\n";
        }

        void timer_Tick(object sender, EventArgs e)
        {
            float cost;
            string gesture = sketchTyping.GetMatchingCommand(inputText, trainData, out cost);
            if (cost <= 1 - threshold)
                Action(gesture);
            DebugLog(string.Format("inputText = {0}, gesture = {1}, const = {2}\n", inputText, gesture, cost));
            inputText = "";
            timer.Enabled = false;
        }

        void Action(string gesture)
        {
            SendKeys.Send(gesture);
        }

        //----------------------------------------------------------

        private void startButton_Click(object sender, EventArgs e)
        {
            hooker.OnKeyHook = OnKeyHook;
            hooker.Hook();
            UpdateGUI();
            DebugLog("Start hooking");
        }

        private void stopButton_Click(object sender, EventArgs e)
        {
            hooker.Unhook();
            UpdateGUI();
            DebugLog("Stop hooking");
        }

        string inputText = "";
        public bool OnKeyHook(int code, WM wParam, KBDLLHOOKSTRUCT lParam, Hooker hooker)
        {
            switch (wParam)
            {
                case WM.KEYDOWN:
                case WM.SYSKEYDOWN:
                    try
                    {
                        if (hooker.onAlt || hooker.onCtrl || hooker.onFn || hooker.onShift || hooker.onWin)
                            return false;

                        if (sketchTyping.keyPointsDict.ContainsKey(char.ToLower((char)lParam.vkCode)))
                        {
                            timer.Enabled = false;
                            inputText += (char)lParam.vkCode;
                            timer.Enabled = true;
                            timer.Start();
                        }

                        return false;
                    }
                    catch (Exception e)
                    {
                        Console.WriteLine(e);
                    }
                    break;
            }
            return false;
        }
        
        //----------------------------------------------------------

        private void loadButton_Click(object sender, EventArgs e)
        {
            if (File.Exists(gestureFile))
            {
                trainData = SketchTypeCommand.LoadCommands(gestureFile, 64, 64);
                DebugLog("load gestures: # = " + trainData.Count + "\n");
            }
        }

        private void editButton_Click(object sender, EventArgs e)
        {
            string path = Path.GetFullPath("SketchTypinDataCollect.exe");
            if (File.Exists(path) && File.Exists(gestureFile))
                Process.Start(path, gestureFile);
        }

        private void gestureComboBox_TextChanged(object sender, EventArgs e)
        {
            gestureFile = GetGestureFile();
            UpdateGUI();
        }

        string GetGestureFile()
        {
            if (string.IsNullOrWhiteSpace(gestureComboBox.Text))
                return "";
            string path = Path.GetFullPath(gestureComboBox.Text);
            if (File.Exists(path))
                return path;
            return "";
        }

        private void thresholdNumeriUpDown_ValueChanged(object sender, EventArgs e)
        {
            threshold = GetThreshold();
        }

        private double GetThreshold()
        {
            return Math.Max(0, Math.Min(1, 0.01 * (double)thresholdNumeriUpDown.Value));
        }

        double threshold = 0.8;

        //---------------------------------------------------

        private void UpdateGUI()
        {
            if (hooker.isHooking)
            {
                startButton.Enabled = false;
                stopButton.Enabled = true;
            }
            else
            {
                startButton.Enabled = true;
                stopButton.Enabled = false;
            }

            if (!File.Exists(gestureFile))
                loadButton.Enabled = false;
            else
                loadButton.Enabled = true;

            if (!File.Exists("SketchTypinDataCollect.exe"))
                editButton.Enabled = false;
            else
                editButton.Enabled = true;
        }

    }
}

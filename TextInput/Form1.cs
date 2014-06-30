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

namespace TextInput
{
    public partial class Form1 : Form
    {
        public Hooker hooker = new Hooker(); 
        public FLib.SketchTyping sketchTyping;
        public List<SketchTypeCommand> trainData = new List<SketchTypeCommand>();

        string gestureFile = "";

        bool alwaysRecognition = true;
        Timer timer = new Timer();


        public Form1()
        {
            InitializeComponent();
        }

        private void Form1_Load(object sender, EventArgs e)
        {
            gestureFile = GetGestureFile();
            alwaysRecognition = GetRecognitionMode();
            threshold = GetThreshold();
            shortcutKey = GetShortcutKey();

            if (File.Exists("keyboard.png"))
            {
                using (var bmp = new Bitmap("keyboard.png"))
                    sketchTyping = new SketchTyping(new Bitmap(bmp));
            }
            else
            {
                MessageBox.Show("キーボード画像（\"keyboard.png\"）が必要です");
                Application.Exit();
            }

            if (Directory.Exists("Gestures"))
                gestureComboBox.Items.AddRange(Directory.GetFiles("Gestures").Where(f => f.EndsWith(".txt")).ToArray());

            gestureComboBox.Text = Path.GetFullPath("Gestures/gestureSimple.txt");

            for (int i = 'A'; i <= 'Z'; i++)
                keyCombobox.Items.Add("" + (char)i);

            timer.Interval = 250;
            timer.Tick += new EventHandler(timer_Tick);

            UpdateGUI();
        }

        void timer_Tick(object sender, EventArgs e)
        {
            float cost;
            string gesture = sketchTyping.GetMatchingCommand(inputText, trainData, out cost);
            Action(gesture);
            textBox.Text += string.Format("inputText = {0}, gesture = {1}, const = {2}\n", inputText, gesture, cost);
            inputText = "";
            timer.Enabled = false;
        }

        void Action(string gesture)
        {
            Clipboard.Clear();
            Clipboard.SetText(gesture);
            System.Threading.Thread.Sleep(100);
            SendKeys.Send("^v");
        }

        //----------------------------------------------------------

        private void startButton_Click(object sender, EventArgs e)
        {
            hooker.OnKeyHook = OnKeyHook;
            hooker.Hook();
            UpdateGUI();
        }

        private void stopButton_Click(object sender, EventArgs e)
        {
            hooker.Unhook();
            UpdateGUI();
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
                        if (CanRecognize((int)lParam.vkCode))
                        {
                            if (hooker.onAlt || hooker.onCtrl || hooker.onFn || hooker.onShift || hooker.onWin)
                                return true;

                            if (sketchTyping.keyPointsDict.ContainsKey(char.ToLower((char)lParam.vkCode)))
                            {
                                timer.Enabled = false;
                                inputText += (char)lParam.vkCode;
                                timer.Enabled = true;
                                timer.Start();
                            }
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

        bool CanRecognize(int vkCode)
        {
            if (alwaysRecognition)
                return true;

            if (shortcutKey == null)
                return false;

            if (!hooker.isHooking)
                return false;

            if (shortcutKey.ctrl && !hooker.onCtrl)
                return false;

            if (shortcutKey.alt && !hooker.onAlt)
                return false;

            if (shortcutKey.shift && !hooker.onShift)
                return false;

            if (shortcutKey.key == "" || (char.ToUpper(shortcutKey.key[0]) != vkCode && char.ToLower(shortcutKey.key[0]) != vkCode))
                return false;

            return true;
        }
        
        //----------------------------------------------------------

        private void loadButton_Click(object sender, EventArgs e)
        {
            if (File.Exists(gestureFile))
            {
                trainData = SketchTypeCommand.LoadCommands(gestureFile, 64, 64);
                textBox.Text += "load gestures: # = " + trainData.Count + "\n";
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

        //------------------------------------------------------

        private void alwaysRButton_CheckedChanged(object sender, EventArgs e)
        {
            alwaysRecognition = GetRecognitionMode();
            UpdateGUI();
        }

        private void shortcutRButton_CheckedChanged(object sender, EventArgs e)
        {
            alwaysRecognition = GetRecognitionMode();
            UpdateGUI();
        }

        bool GetRecognitionMode()
        {
            return alwaysRButton.Checked;
        }

        public class ShortcutKey
        {
            public bool ctrl = false;
            public bool shift = false;
            public bool alt = false;
            public string key = "";
            public ShortcutKey(bool ctrl, bool shift, bool alt, string key)
            {
                this.ctrl = ctrl;
                this.shift = shift;
                this.alt = alt;
                this.key = key;
            }
        }

        private void numericUpDown1_ValueChanged(object sender, EventArgs e)
        {
            threshold = GetThreshold();
        }

        private double GetThreshold()
        {
            return Math.Max(0, Math.Min(1, 0.01 * (double)thresholdNumeriUpDown.Value));
        }

        double threshold = 0.8;

        ShortcutKey GetShortcutKey()
        {
            if (keyCombobox.Text.Length != 1)
                return null;

            if (keyCombobox.Text[0] < 'A' || 'Z' < keyCombobox.Text[0])
                return null;

            return new ShortcutKey(
                ctrlCheckbox.Checked,
                shiftCheckbox.Checked,
                altCheckbox.Checked,
                keyCombobox.Text);
        }

        ShortcutKey shortcutKey = null;

        private void ctrlCheckbox_CheckedChanged(object sender, EventArgs e)
        {
            shortcutKey = GetShortcutKey();
        }

        private void shiftCheckbox_CheckedChanged(object sender, EventArgs e)
        {
            shortcutKey = GetShortcutKey();
        }

        private void altCheckbox_CheckedChanged(object sender, EventArgs e)
        {
            shortcutKey = GetShortcutKey();
        }

        private void keyCombobox_TextChanged(object sender, EventArgs e)
        {
            shortcutKey = GetShortcutKey();
        }


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

 
            if (alwaysRecognition)
            {
                thresholdNumeriUpDown.Enabled = true;
                ctrlCheckbox.Enabled = altCheckbox.Enabled = shiftCheckbox.Enabled = keyCombobox.Enabled = false;
            }
            else
            {
                thresholdNumeriUpDown.Enabled = false;
                ctrlCheckbox.Enabled = altCheckbox.Enabled = shiftCheckbox.Enabled = keyCombobox.Enabled = true;
            }
        }

    }
}

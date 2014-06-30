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

        Timer timer = new Timer();

        public Form1()
        {
            InitializeComponent();
        }

        private void Form1_Load(object sender, EventArgs e)
        {
            // init variables
            gestureFile = Path.GetFullPath("Gestures/gestureSimple.txt");
            shortcutKey = GetShortcutKey();

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

            for (int i = 'A'; i <= 'Z'; i++)
                keyCombobox.Items.Add("" + (char)i);

            // load train data
            if (!File.Exists(gestureFile))
            {
                MessageBox.Show("ジェスチャファイル " + gestureFile + " が見つかりません");
                Application.Exit();
            }
            trainData = SketchTypeCommand.LoadCommands(gestureFile, 64, 64);

            DebugLog("load gestures: # = " + (trainData == null ? "null" : "" + trainData.Count));

            // set timer
            timer.Interval = 300;
            timer.Tick += new EventHandler(timer_Tick);

            UpdateGUI();
        }

        void DebugLog(string mes)
        {
            log.Text = mes;
            textBox.Text += mes + "\n";
        }

        void timer_Tick(object sender, EventArgs e)
        {
            float cost;
            string gesture = sketchTyping.GetMatchingCommand(inputText, trainData, out cost);
            DebugLog(string.Format("inputText = {0}, gesture = {1}, const = {2}", inputText, gesture, cost));
            recogMode = false;
            if (recogMode)
                DebugLog("Finish gesture recognition");
            inputText = "";
            timer.Enabled = false;

            Action(gesture);
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

        bool recogMode = false;
        string inputText = "";
        public bool OnKeyHook(int code, WM wParam, KBDLLHOOKSTRUCT lParam, Hooker hooker)
        {
            switch (wParam)
            {
                case WM.KEYDOWN:
                case WM.SYSKEYDOWN:
                    try
                    {
                        if (recogMode)
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
                        else
                        {
                            recogMode = CanRecognize((int)lParam.vkCode);
                            if (recogMode)
                                DebugLog("Start gesture recognition");
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
                DebugLog("load gestures: # = " + trainData.Count);
            }
        }

        private void editButton_Click(object sender, EventArgs e)
        {
            string path = Path.GetFullPath("SketchTypingDataCollect.exe");
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

            if (!File.Exists("SketchTypingDataCollect.exe"))
                editButton.Enabled = false;
            else
                editButton.Enabled = true;

        }
    }
}
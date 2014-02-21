using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Windows.Forms;
using FLib;

namespace SketchTypingBrowser
{
    public partial class Form1 : Form
    {
        const string host = "127.0.0.1";
        const int port = 8094;
        const string gesturePath = "../../../Resource/gestureBrows.txt";
        System.Diagnostics.Process server = null;
        SketchTypingClient client;
        Timer timer = new Timer();

        public Form1()
        {
            InitializeComponent();
        }

        private void Form1_Load(object sender, EventArgs e)
        {

        }

        void timer_Tick(object sender, EventArgs e)
        {
            if (client == null) return;

            string text = client.ReadString();

            try
            {
                if (text != "" && !text.StartsWith("StartInput") && !text.StartsWith("TabPressed"))
                {
                    label1.Text = text;
                    SendKeys.SendWait(text);
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex + ":" + ex.StackTrace);
            }
            client.SendReceivedSignal();
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

        private void checkBox1_CheckedChanged(object sender, EventArgs e)
        {
            if (checkBox1.Checked)
            {
                CreateSketchTyping();
            }
            else
            {
                DisposeSketchTyping();
            }
        }
    }
}

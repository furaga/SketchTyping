using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Drawing;
using System.Data;
using System.Linq;
using System.Text;
using System.Windows.Forms;
using Extensibility;
using EnvDTE;
using EnvDTE80;
using Microsoft.VisualStudio.CommandBars;
using System.Resources;
using System.Reflection;
using System.Globalization;
using FLib;

namespace SketchTypingVSAddin
{
    public partial class SketchTypingControl : UserControl
    {
        const string host = "127.0.0.1";
        const int port = 8092;
        const string gesturePath = "../../../Resource/gestureVS.txt";

        public DTE2 _applicationObject { set; private get; }
        System.Diagnostics.Process server = null;
        SketchTypingClient client;
        Timer timer = new Timer();

        public SketchTypingControl()
        {
            InitializeComponent();
        }

        public void Initialize(DTE2 _applicationObject)
        {
            try
            {
                this._applicationObject = _applicationObject;
                string serverDir = System.IO.Path.GetDirectoryName(_applicationObject.AddIns.Item(1).SatelliteDllPath);
                string serverPath = serverDir + '\\' + SketchTypingServer.serverPath;
                server = new System.Diagnostics.Process();
                server.StartInfo = new System.Diagnostics.ProcessStartInfo(serverPath, host + " " + port + " " + gesturePath)
                {
                    WorkingDirectory = serverDir
                };
                server.Start();
                client = new SketchTypingClient(host, port);
                timer.Interval = 66;
                timer.Tick += new EventHandler(timer_Tick);
                timer.Enabled = true;
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message);
            }
        }

        void timer_Tick(object sender, EventArgs e)
        {
            if (client == null) return;

            string text = client.ReadString();
            try
            {
                if (text != "" && !text.StartsWith("StartInput"))
                {
                    if (_applicationObject.ActiveDocument != null)
                    {
                        TextSelection textSelection = (TextSelection)_applicationObject.ActiveDocument.Selection;
                        if (textSelection != null)
                        {
                            EditPoint startPoint = textSelection.TopPoint.CreateEditPoint();
                            EditPoint endPoint = textSelection.BottomPoint.CreateEditPoint();

                            while (!startPoint.AtStartOfDocument)
                            {
                                startPoint.CharLeft(1);
                                if (char.IsWhiteSpace(startPoint.GetText(1)[0]))
                                {
                                    startPoint.CharRight(1);
                                    break;
                                }
                            }

                            startPoint.Delete(endPoint);
                            endPoint.Insert(text);

                            // 整形
                            endPoint.StartOfDocument();
                            startPoint.EndOfDocument();
                            endPoint.SmartFormat(startPoint);
                        }
                    }
                    richTextBox1.Text = text;
                }
            }
            catch (Exception)
            {
                //                MessageBox.Show(ex + ":" + ex.StackTrace);
            }
            client.SendReceivedSignal();
        }

        public void Dispose()
        {
            // 
            try
            {
                if (client != null)
                {
                    client.Dispose();
                }

                if (server != null)
                {
                    server.Kill();
                }

                base.Dispose();
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex + ":" + ex.StackTrace);
            }
        }
    }
}

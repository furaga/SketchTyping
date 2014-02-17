using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using System.Windows.Threading;
using FLib;

namespace Company.SketchTypinVSExtension
{
    /// <summary>
    /// Interaction logic for MyControl.xaml
    /// </summary>
    public partial class SketchTypingControlWPF : UserControl
    {
        const string host = "127.0.0.1";
        const int port = 8093;
        const string gesturePath = "../../../Resource/gestureVS.txt";

        System.Diagnostics.Process server = null;
        SketchTypingClient client;
        DispatcherTimer timer = new DispatcherTimer();

        public SketchTypingControlWPF()
        {
            InitializeComponent();
            Width = Height = 300;
            textBox2.Text = @"C:\Users\furag_000\Dropbox\Research\SketchIntellisence\src\SketchTyping\SketchTypingServer\bin\Debug";

            try
            {
                timer.Interval = new TimeSpan(0, 0, 0, 1, 0);
                timer.Tick += new EventHandler(timer_Tick);
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message);
            }
        }

        EnvDTE.Document ActiveDocument { get { return SketchTypingVSExtension.TextAdornment1Factory.dte2.ActiveDocument; } }

        int syntaxRemedyWordCount = 0;
        const string StartSyntaxWord = @"/*[*/";
        const string EndSyntaxWord = @"/*]*/";
        
        void timer_Tick(object sender, EventArgs e)
        {
            if (client == null) return;

            string text = client.ReadString();
            try
            {
                if (text != "" && !text.StartsWith("StartInput") && ActiveDocument != null)
                {
                    switch (text)
                    {
                        case "TabPressed":
//                            if (syntaxRemedyWordCount >= 1)
                            {
                                if (NextSyntaxWord()) syntaxRemedyWordCount--;
                                else syntaxRemedyWordCount = 0;
                            }
                            break;
                        default:
                            InsertSyntax(text.Trim());
                            syntaxRemedyWordCount += text.Length - text.Replace(StartSyntaxWord, StartSyntaxWord.Substring(0, StartSyntaxWord.Length - 1)).Length;
                            (ActiveDocument.Selection as EnvDTE.TextSelection).StartOfDocument();
                            goto case "TabPressed";
                    }

                    textBox1.Text = text;
                }
            }
            catch (Exception)
            {
                //                MessageBox.Show(ex + ":" + ex.StackTrace);
            }
            client.SendReceivedSignal();
        }

        void InsertSyntax(string syntax)
        {
            EnvDTE.TextSelection textSelection = ActiveDocument.Selection as EnvDTE.TextSelection;
            if (textSelection != null)
            {
                EnvDTE.EditPoint startPoint = textSelection.TopPoint.CreateEditPoint();
                EnvDTE.EditPoint endPoint = textSelection.BottomPoint.CreateEditPoint();

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
                endPoint.Insert(syntax);

                // 整形
                endPoint.StartOfDocument();
                startPoint.EndOfDocument();
                endPoint.SmartFormat(startPoint);
            }
        }

        bool NextSyntaxWord()
        {
            EnvDTE.TextSelection textSelection = ActiveDocument.Selection as EnvDTE.TextSelection;
            if (textSelection != null)
            {
                EnvDTE.EditPoint startPoint = textSelection.TopPoint.CreateEditPoint();
                EnvDTE.EditPoint endPoint = textSelection.BottomPoint.CreateEditPoint();

                // 直前のタブを消す
                for (int i = 0; i < 4; i++)
                {
                    if (startPoint.AtStartOfDocument) break;
                    startPoint.CharLeft(1);
                    if (!char.IsWhiteSpace(startPoint.GetText(1)[0]))
                    {
                        startPoint.CharRight(1);
                        break;
                    }
                }
                startPoint.Delete(endPoint);

                EnvDTE.EditPoint newStartPoint = startPoint.CreateEditPoint();
                bool flg1 = newStartPoint.FindPattern(StartSyntaxWord);
                EnvDTE.EditPoint newEndPoint = newStartPoint.CreateEditPoint();
                bool flg2 = newEndPoint.FindPattern(EndSyntaxWord);
                if (flg1 && flg2)
                {
                    textSelection.MoveToAbsoluteOffset(newEndPoint.AbsoluteCharOffset + EndSyntaxWord.Length);
                    textSelection.MoveToAbsoluteOffset(newStartPoint.AbsoluteCharOffset, true);
                    return true;
                }
            }
            return false;
        }

        private void checkBox1_Checked(object sender, RoutedEventArgs e)
        {
            string serverDir = textBox2.Text;
            string serverPath = serverDir + '\\' + SketchTypingServer.serverPath;
            server = new System.Diagnostics.Process();
            server.StartInfo = new System.Diagnostics.ProcessStartInfo(serverPath, host + " " + port + " " + gesturePath)
            {
                WorkingDirectory = serverDir
            };
            server.Start();
            client = new SketchTypingClient(host, port);

            textBox1.Text = checkBox1.IsChecked + "";
            timer.Start();
        }

        private void checkBox1_Unchecked(object sender, RoutedEventArgs e)
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
            textBox1.Text = checkBox1.IsChecked + "";
        }
    }
}
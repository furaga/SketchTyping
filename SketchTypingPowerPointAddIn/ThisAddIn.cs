using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Xml.Linq;
using PowerPoint = Microsoft.Office.Interop.PowerPoint;
using Office = Microsoft.Office.Core;
using System.Windows.Forms;
using System.Drawing;
using System.IO;
using FLib;
//using SketchTypingLib;

namespace SketchTypingPowerPointAddIn
{
    public partial class ThisAddIn
    {
        System.Diagnostics.Process server;
        SketchTypingClient client;
        Timer timer = new Timer();

        private void ThisAddIn_Startup(object sender, System.EventArgs e)
        {
            string host = "127.0.0.1";
            int port = 8091;
            string gesturePath = "../../../Resource/gesturePowerPoint.txt";
            server = System.Diagnostics.Process.Start(System.IO.Path.GetFullPath(SketchTypingServer.serverPath), host + " " + port + " " + gesturePath);
            while (true)
            {
                try
                {
                    client = new SketchTypingClient(host, port);
                    timer.Interval = 66;
                    timer.Tick += new EventHandler(timer_Tick);
                    timer.Enabled = true;
                    break;
                }
                catch (Exception)
                {
                    Console.WriteLine("Retry");
                }
            }
        }

        PowerPoint.Slide CurrentSlide
        {
            get
            {
                return Application.ActivePresentation.Slides[Application.ActiveWindow.View.Slide.SlideIndex];
            }
        }

        PowerPoint.ShapeRange FocusedShapes
        {
            get
            {
                try
                {
                    return Application.ActiveWindow.Selection.ShapeRange;
                }
                catch (Exception)
                {
                    return null;
                }
            }
        }

        List<Tuple<long, string>> textHistory = new List<Tuple<long,string>>();

        void timer_Tick(object sender, EventArgs e)
        {
            string text = client.ReadString();
            PowerPoint.Shape newShape = null;

            try
            {
                if (text != "")
                {
                    Point offset = Point.Empty;
                    if (FocusedShapes != null &&FocusedShapes.Count >= 1)
                    {
                        offset.X = (int)FocusedShapes[1].Left + 10;
                        offset.Y = (int)FocusedShapes[1].Top + 10;
                    }

                    //
                    if (FocusedShapes != null)
                    {
                        textHistory.Add(new Tuple<long, string>(DateTime.Now.Ticks, FocusedShapes[1].TextFrame.TextRange.Text));
                    }
                    if (textHistory.Count >= 100)
                    {
                        textHistory.RemoveAt(0);
                    }

                    //
                    if (text.StartsWith("StartInput"))
                    {
                        long ticks;
                        if (long.TryParse(text.Replace("StartInput", ""), out ticks))
                            for (int i = textHistory.Count - 1; i >= 0; i--)
                                if (textHistory[i].Item1 <= ticks)
                                {
                                    tmpText = textHistory[i].Item2;
                                    break;
                                }
                    }

                    switch (text)
                    {
                        case "Ellipse":
                            newShape = CurrentSlide.Shapes.AddShape(Office.MsoAutoShapeType.msoShapeOval, offset.X, offset.Y, 310, 250);
                            goto case "CancelText";
                        case "Rectangle":
                            newShape = CurrentSlide.Shapes.AddShape(Office.MsoAutoShapeType.msoShapeRectangle, offset.X, offset.Y, 440, 160);
                            goto case "CancelText";
                        case "Triangle":
                            newShape = CurrentSlide.Shapes.AddShape(Office.MsoAutoShapeType.msoShapeIsoscelesTriangle, offset.X, offset.Y, 180, 270);
                            goto case "CancelText";
                        case "TextBox":
                            newShape = CurrentSlide.Shapes.AddTextbox(Office.MsoTextOrientation.msoTextOrientationHorizontal, offset.X, offset.Y, 400, 30);
                            goto case "CancelText";
                        case "LeftArrow":
                            newShape = CurrentSlide.Shapes.AddShape(Office.MsoAutoShapeType.msoShapeLeftArrow, offset.X, offset.Y, 95, 85);
                            goto case "CancelText";
                        case "RightArrow":
                            newShape = CurrentSlide.Shapes.AddShape(Office.MsoAutoShapeType.msoShapeRightArrow, offset.X, offset.Y, 95, 85);
                            goto case "CancelText";
                        case "UpArrow":
                            newShape = CurrentSlide.Shapes.AddShape(Office.MsoAutoShapeType.msoShapeUpArrow, offset.X, offset.Y, 85, 95);
                            goto case "CancelText";
                        case "DownArrow":
                            newShape = CurrentSlide.Shapes.AddShape(Office.MsoAutoShapeType.msoShapeDownArrow, offset.X, offset.Y, 85, 95);
                            goto case "CancelText";
                        case "Expand":
                            for (int i = 1; i <= FocusedShapes.Count; i++)
                            {
                                FocusedShapes[i].ScaleWidth(1.2f, Office.MsoTriState.msoFalse, Office.MsoScaleFrom.msoScaleFromMiddle);
                                FocusedShapes[i].ScaleHeight(1.2f, Office.MsoTriState.msoFalse, Office.MsoScaleFrom.msoScaleFromMiddle);
                            }
                            goto case "CancelText";
                        case "Shrink":
                            for (int i = 1; i <= FocusedShapes.Count; i++)
                            {
                                FocusedShapes[i].ScaleWidth(0.9f, Office.MsoTriState.msoFalse, Office.MsoScaleFrom.msoScaleFromMiddle);
                                FocusedShapes[i].ScaleHeight(0.9f, Office.MsoTriState.msoFalse, Office.MsoScaleFrom.msoScaleFromMiddle);
                            }
                            goto case "CancelText";
                        case "FillRed":
                            for (int i = 1; i <= FocusedShapes.Count; i++)
                            {
                                FocusedShapes[i].Fill.ForeColor.RGB = Color.FromArgb(0, 0, 255).ToArgb();
                            }
                            goto case "CancelText";
                        case "FillYellow":
                            for (int i = 1; i <= FocusedShapes.Count; i++)
                            {
                                FocusedShapes[i].Fill.ForeColor.RGB = Color.FromArgb(0, 255, 255).ToArgb();
                            }
                            goto case "CancelText";
                        case "FillWhite":
                            for (int i = 1; i <= FocusedShapes.Count; i++)
                            {
                                FocusedShapes[i].Fill.ForeColor.RGB = Color.FromArgb(255, 255, 255).ToArgb();
                            }
                            goto case "CancelText";
                        case "CancelText":
                            if (FocusedShapes.Count >= 1)
                            {
                                FocusedShapes[1].TextFrame.TextRange.Text = "";//  tmpText;
                            }
                            if (newShape != null)
                            {
                                newShape.Select();
                            }
                            break;
                        default:
                            break;
                    }
                }
            }
            catch (Exception ex)
            {
//                MessageBox.Show(ex + ":" + ex.StackTrace);
            }

            client.SendReceivedSignal();
        }

        string tmpText = "";

        private void ThisAddIn_Shutdown(object sender, System.EventArgs e)
        {
            if (server != null)
            {
                client.Dispose();
                server.Kill();
            }
        }

        #region VSTO generated code

        private void InternalStartup()
        {
            this.Startup += new System.EventHandler(ThisAddIn_Startup);
            this.Shutdown += new System.EventHandler(ThisAddIn_Shutdown);
        }
        
        #endregion
    }
}

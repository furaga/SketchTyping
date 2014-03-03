using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Drawing;
using System.Data;
using System.Linq;
using System.Text;
using System.Windows.Forms;
using FLib;

namespace Company.SketchTypinVSExtension
{
    public partial class AnnotationSketchControl : UserControl
    {
        EnvDTE.Document ActiveDocument
        {
            get
            {
                if (SketchTypingVSExtension.TextAdornment1Factory.dte2 == null) return null;
                return SketchTypingVSExtension.TextAdornment1Factory.dte2.ActiveDocument;
            }
        }

        string SolutionDir
        {
            get
            {
                try
                {
                    var dte2 = SketchTypingVSExtension.TextAdornment1Factory.dte2;
                    if (dte2 == null || dte2.Solution == null) return "";
                    return System.IO.Path.GetDirectoryName(dte2.Solution.FullName);
                }
                catch (Exception)
                {
                    return "";
                }
            }
        }

        const int SketchWidth = 400;
        const int SketchHeight = 300;

        Bitmap Bmp;
        Pen pen = new Pen(Brushes.Black, 3);
        List<List<Point>> sketch = new List<List<Point>>();
        int strokePointer = 0;
        bool drawing = false;

        public AnnotationSketchControl()
        {
            InitializeComponent();
            Bmp = new Bitmap(SketchWidth, SketchHeight);
            using (Graphics g = Graphics.FromImage(Bmp))
            {
                g.Clear(Color.White);
            }
            canvas.Invalidate();
        }

        private void canvas_MouseDown(object sender, MouseEventArgs e)
        {
            if (e.Button == System.Windows.Forms.MouseButtons.Left)
            {
                List<Point> stroke = new List<Point>();
                strokePointer++;
                if (sketch.Count <= strokePointer)
                {
                    sketch.Add(stroke);
                    strokePointer = sketch.Count;
                }
                else
                {
                    sketch[strokePointer] = stroke;
                    if (strokePointer + 1 < sketch.Count)
                    {
                        sketch.RemoveRange(strokePointer + 1, sketch.Count - strokePointer - 1);
                    }
                }
                AddPointToStroke(e.Location);
                drawing = true;
                canvas.Invalidate();
            }
        }

        private void canvas_MouseMove(object sender, MouseEventArgs e)
        {
            if (e.Button == System.Windows.Forms.MouseButtons.Left)
            {
                if (drawing)
                {
                    AddPointToStroke(e.Location);
                    canvas.Invalidate();
                }
            }
        }

        private void canvas_MouseUp(object sender, MouseEventArgs e)
        {
            if (e.Button == System.Windows.Forms.MouseButtons.Left)
            {
                AddPointToStroke(e.Location);
                drawing = false;
                canvas.Invalidate();
            }
        }

        private void canvas_MouseClick(object sender, MouseEventArgs e)
        {
            if (e.Button == System.Windows.Forms.MouseButtons.Right)
            {

            }
        }

        private void contextMenuStrip_Opening(object sender, CancelEventArgs e)
        {
            contextMenuStrip.Items.Clear();
            if (ActiveDocument != null && SolutionDir != "")
            {
                var select = (ActiveDocument.Selection as EnvDTE.TextSelection);
                if (select != null)
                {
                    var start = select.TopPoint.CreateEditPoint();
                    var end = select.BottomPoint.CreateEditPoint();
                    var match = EditPointToFunctionDefinition(ref start, ref end);
                    if (match.Success)
                    {
                        string args = match.Groups["Arguments"].Value;
                        var tokens =
                            args.Split(',', ' ')
                            .Where(t => !string.IsNullOrWhiteSpace(t))
                            .Where((_, i) => i % 2 == 1) // 引数名部分
                            .ToList();
                        foreach (string t in tokens)
                        {
                            contextMenuStrip.Items.Add(t);
                        }
                    }
                }
            }
        }

        private void contextMenuStrip_ItemClicked(object sender, ToolStripItemClickedEventArgs e)
        {
            textBox1.Text = e.ClickedItem.Text;
        }

        private void canvas_Paint(object sender, PaintEventArgs e)
        {
            if (Bmp != null)
            {
                e.Graphics.DrawImage(Bmp, Point.Empty);
            }
        }

        private void AddPointToStroke(Point pt)
        {
            if (sketch.Count <= 1) return;
            if (pt.X < 0 || SketchWidth <= pt.X) return;
            if (pt.Y < 0 || SketchHeight <= pt.Y) return;
            var stroke = sketch.Last();
            stroke.Add(pt);
            if (stroke.Count >= 2)
            {
                using (Graphics g = Graphics.FromImage(Bmp))
                {
                    g.DrawLine(pen, stroke[stroke.Count - 2], stroke[stroke.Count - 1]);
                }
            }
        }

        void RefleshSketch()
        {
            using (Graphics g = Graphics.FromImage(Bmp))
            {
                g.Clear(Color.White);
                for (int i = 0; i <= strokePointer; i++)
                {
                    var stroke = sketch[i];
                    if (stroke.Count >= 2)
                    {
                        g.DrawLines(pen, stroke.ToArray());
                    }
                }
            }
        }

        System.Text.RegularExpressions.Match EditPointToFunctionDefinition(ref EnvDTE.EditPoint start, ref EnvDTE.EditPoint end)
        {
            start.StartOfDocument();
            bool found = end.FindPattern("{");
            if (!found) end.EndOfDocument();
            string code = start.GetText(end);

            var match = System.Text.RegularExpressions.Regex.Match(
                code,
                SimpleParser.FUNCTION_DECL_STR,
                System.Text.RegularExpressions.RegexOptions.Multiline |
                System.Text.RegularExpressions.RegexOptions.RightToLeft);

            return match;
        }

        private void okButton_Click(object sender, EventArgs e)
        {
            if (ActiveDocument != null && SolutionDir != "")
            {
                var select = (ActiveDocument.Selection as EnvDTE.TextSelection);
                if (select != null)
                {
                    var start = select.TopPoint.CreateEditPoint();
                    var end = select.BottomPoint.CreateEditPoint();
                    var match = EditPointToFunctionDefinition(ref start, ref end);
                    if (match.Success)
                    {
                        label1.Text = match.Groups["FunctionName"].Value;

                        // スケッチ情報をテキストとしてファイル保存
                        string sketchDir = System.IO.Path.Combine(SolutionDir, "AnnotationSketches");
                        if (!System.IO.Directory.Exists(sketchDir)) System.IO.Directory.CreateDirectory(sketchDir);
                        string filepath = System.IO.Path.Combine(sketchDir, Guid.NewGuid().ToString() + ".txt");
                        string text = "";
                        foreach (var stroke in sketch)
                        {
                            text += string.Join(" ", stroke.Select(pt => pt.X + "," + pt.Y).ToArray()) + "\n";
                        }
                        System.IO.File.WriteAllText(filepath, text);

                        // スケッチを表示するための文字列（コメント）をコードに追加
                        end.FindPattern(
                            match.Value.TrimStart('}', ';').Trim(),
                            vsFindOptionsValue: (int)EnvDTE.vsFindOptions.vsFindOptionsBackwards);
                        end.Insert(string.Format(
@"/// AnnotationSketch:{0}
", System.IO.Path.GetFileName(filepath)));
                        start = select.TopPoint.CreateEditPoint();
                        end = select.TopPoint.CreateEditPoint();
                        start.StartOfDocument();
                        end.EndOfDocument();
                        start.SmartFormat(end);
                    }
                }

            }
        }

        private void cancelButton_Click(object sender, EventArgs e)
        {
            sketch.Clear();
            using (Graphics g = Graphics.FromImage(Bmp))
            {
                g.Clear(Color.White);
            }
            canvas.Invalidate();
        }

        private void undoButton_Click(object sender, EventArgs e)
        {
            strokePointer = Math.Max(-1, strokePointer - 1);
            RefleshSketch();
            canvas.Invalidate();
        }

        private void redoButton_Click(object sender, EventArgs e)
        {
            strokePointer = Math.Min(sketch.Count - 1, strokePointer + 1);
            RefleshSketch();
            canvas.Invalidate();
        }

        private void refButton_Click(object sender, EventArgs e)
        {
            if (ActiveDocument != null && SolutionDir != "")
            {
                var select = (ActiveDocument.Selection as EnvDTE.TextSelection);
                if (select != null)
                {
                    var start = select.TopPoint.CreateEditPoint();
                    var end = select.BottomPoint.CreateEditPoint();
                    var match = EditPointToFunctionDefinition(ref start, ref end);
                    if (match.Success)
                    {
                        label1.Text = match.Groups["FunctionName"].Value;

                        // スケッチ情報をテキストとしてファイル保存
                        openFileDialog1.RestoreDirectory = true;
                        openFileDialog1.Filter = "*.bmp,*.png,*.jpg|*.bmp;*.png;*.jpg";
                        if (openFileDialog1.ShowDialog() == DialogResult.OK)
                        {
                            string sketchDir = System.IO.Path.Combine(SolutionDir, "AnnotationSketches");
                            if (!System.IO.Directory.Exists(sketchDir)) System.IO.Directory.CreateDirectory(sketchDir);
                            string dst = System.IO.Path.Combine(sketchDir, System.IO.Path.GetFileName(openFileDialog1.FileName));
                            System.IO.File.Copy(openFileDialog1.FileName, dst);
                            // スケッチを表示するための文字列（コメント）をコードに追加
                            end.FindPattern(
                                match.Value.TrimStart('}', ';').Trim(),
                                vsFindOptionsValue: (int)EnvDTE.vsFindOptions.vsFindOptionsBackwards);
                            end.Insert(string.Format("/// AnnotationSketch:{0}\n", System.IO.Path.GetFileName(dst)));
                            start = select.TopPoint.CreateEditPoint();
                            end = select.TopPoint.CreateEditPoint();
                            start.StartOfDocument();
                            end.EndOfDocument();
                            start.SmartFormat(end);
                        }
                    }
                }
            }
        }
    }
}
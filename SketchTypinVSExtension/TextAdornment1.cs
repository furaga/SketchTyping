using System;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using Microsoft.VisualStudio.Text;
using Microsoft.VisualStudio.Text.Editor;
using Microsoft.VisualStudio.Text.Formatting;
using System.Collections.Generic;
using System.ComponentModel;
using FLib;

namespace SketchTypingVSExtension
{
    ///<summary>
    ///TextAdornment1 places red boxes behind all the "A"s in the editor window
    ///</summary>
    public class TextAdornment1
    {
        IAdornmentLayer _layer;
        IWpfTextView _view;
        Brush _brush;
        Pen _pen;

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


        public TextAdornment1(IWpfTextView view)
        {
            _view = view;
            _layer = view.GetAdornmentLayer("TextAdornment1");

            //Listen to any event that changes the layout (text changes, scrolling, etc)
            _view.LayoutChanged += OnLayoutChanged;

            //Create the pen and brush to color the box behind the a's
            Brush brush = new SolidColorBrush(Color.FromArgb(0x20, 0x00, 0x00, 0xff));
            brush.Freeze();
            Brush penBrush = new SolidColorBrush(Colors.Red);
            penBrush.Freeze();
            Pen pen = new Pen(penBrush, 0.5);
            pen.Freeze();

            _brush = brush;
            _pen = pen;
        }

        /// <summary>
        /// On layout change add the adornment to any reformatted lines
        /// </summary>
        private void OnLayoutChanged(object sender, TextViewLayoutChangedEventArgs e)
        {
            //clear the adornment layer of previous adornments
  //          _layer.RemoveAllAdornments();

            foreach (ITextViewLine line in e.NewOrReformattedLines)
            {
                this.CreateVisuals(line);
            }
        }

        /// <summary>
        /// Within the given line add the scarlet box behind the a
        /// </summary>
        private void CreateVisuals(ITextViewLine line)
        {
            if (line.Snapshot != null && 0 < line.Start && line.End < line.Snapshot.Length)
            {
                string code = line.Snapshot.GetText(line.Start, line.Length);
                string trimmedCode = code.Trim();
                if (trimmedCode.StartsWith("///"))
                {
                    string subCode = trimmedCode.TrimStart('/').Trim();
                    if (subCode.StartsWith("AnnotationSketch:"))
                    {
                        string sketchDir = System.IO.Path.Combine(SolutionDir, "AnnotationSketches");
                        string filePath = System.IO.Path.Combine(sketchDir, subCode.Substring("AnnotationSketch:".Length));
                        if (!TextAdornment1Factory.sketchImages.ContainsKey(filePath) && System.IO.File.Exists(filePath))
                        {
                            TextAdornment1Factory.sketchImages[filePath] =
                                BitmapHandler.CreateBitmapSourceFromBitmap(
                                    BitmapHandler.CreateThumbnail(
                                        BitmapHandler.FromSketchFile(
                                            filePath,
                                            400, 300,
                                            new System.Drawing.Pen(System.Drawing.Brushes.Black, 3),
                                            System.Drawing.Color.White
                                        ),
                                        LineTransformSource.ImageWidth, LineTransformSource.ImageHeight
                                    )
                                );
                        }
                        if (TextAdornment1Factory.sketchImages.ContainsKey(filePath))
                        {
                            SnapshotSpan span = new SnapshotSpan(
                                _view.TextSnapshot,
                                Span.FromBounds(line.Start + (code.Length - subCode.Length + "AnnotationSketch:".Length),
                                line.End));
                            Geometry g = _view.TextViewLines.GetMarkerGeometry(span);
                            if (g != null)
                            {
                                
                                GeometryDrawing drawing = new GeometryDrawing(_brush, _pen, g);
                                drawing.Freeze();
                                DrawingImage drawingImage = new DrawingImage(drawing);
                                drawingImage.Freeze();
                                
                                Image image = new Image();
                                image.Source = TextAdornment1Factory.sketchImages[filePath];

                                //Align the image with the top of the bounds of the text geometry
                                Canvas.SetLeft(image, g.Bounds.Left);
                                Canvas.SetTop(image, g.Bounds.Bottom - LineTransformSource.ImageHeight);

                                _layer.AddAdornment(AdornmentPositioningBehavior.TextRelative, span, null, image, null);
                            }
                        }
                    }
                }
            }
        }
    }
}
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using Microsoft.VisualStudio.Text;
using Microsoft.VisualStudio.Text.Editor;
using Microsoft.VisualStudio.Text.Formatting;

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
            return;
        }
    }
}

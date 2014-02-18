using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.VisualStudio.Text.Editor;
using Microsoft.VisualStudio.Text.Formatting;

namespace SketchTypingVSExtension
{
    internal class LineTransformSource : ILineTransformSource
    {
        public const int ImageWidth= 400;
        public const int ImageHeight = 100;
        IWpfTextView textView;
        public LineTransformSource(IWpfTextView textView)
        {
            this.textView = textView;
        }

        LineTransform ILineTransformSource.GetLineTransform(ITextViewLine line, double yPosition, ViewRelativePosition placement)
        {
            if (textView != null)
            {
                if (line.Start < line.End && 0 < line.Start && line.End < textView.TextSnapshot.Length)
                {
                    string code = line.Snapshot.GetText(line.Start, line.Length).Trim();
                    if (code.StartsWith("///"))
                    {
                        string subCode = code.TrimStart('/').Trim();
                        if (subCode.StartsWith("AnnotationSketch:"))
                        {
                            return new LineTransform(80, 0, 1.0);
                        }
                    }
                }
            }

            return new LineTransform(0, 0, 1.0);
        }
    }
}
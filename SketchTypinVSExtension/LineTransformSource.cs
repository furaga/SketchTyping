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
        private const int ImageAdornmentSpacePadding = 20;

        public LineTransformSource()
        {

        }

        LineTransform ILineTransformSource.GetLineTransform(ITextViewLine line, double yPosition, ViewRelativePosition placement)
        {
            return new LineTransform(0, 0, 1.0);
        }
    }
}
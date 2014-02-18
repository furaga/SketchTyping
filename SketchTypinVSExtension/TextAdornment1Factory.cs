using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows.Media.Imaging;
using System.ComponentModel.Composition;
using Microsoft.VisualStudio.Text.Classification;
using Microsoft.VisualStudio.Text.Editor;
using Microsoft.VisualStudio.Utilities;
using Microsoft.VisualStudio.Text.Formatting;
using EnvDTE;
using EnvDTE80;
using Microsoft.VisualStudio.Shell;

namespace SketchTypingVSExtension
{
    #region Adornment Factory
    /// <summary>
    /// Establishes an <see cref="IAdornmentLayer"/> to place the adornment on and exports the <see cref="IWpfTextViewCreationListener"/>
    /// that instantiates the adornment on the event of a <see cref="IWpfTextView"/>'s creation
    /// </summary>
    [Export(typeof(ILineTransformSourceProvider))]
    [Export(typeof(IWpfTextViewCreationListener))]
    [ContentType("CSharp")]
    [TextViewRole(PredefinedTextViewRoles.Document)]
    internal sealed class TextAdornment1Factory : ILineTransformSourceProvider, IWpfTextViewCreationListener
    {
        public static IWpfTextView _view;
        public static DTE2 dte2;
        public static Dictionary<string, BitmapSource> sketchImages = new Dictionary<string, BitmapSource>();


        [Import]
        internal SVsServiceProvider ServiceProvider = null;

        
        /// <summary>
        /// Defines the adornment layer for the adornment. This layer is ordered 
        /// after the selection layer in the Z-order
        /// </summary>
        [Export(typeof(AdornmentLayerDefinition))]
        [Name("TextAdornment1")]
        [Order(After = PredefinedAdornmentLayers.Text)]
        [TextViewRole(PredefinedTextViewRoles.Document)]
        public AdornmentLayerDefinition editorAdornmentLayer = null;

        ILineTransformSource ILineTransformSourceProvider.Create(IWpfTextView view)
        {
            return new LineTransformSource(view);
        }

        /// <summary>
        /// Instantiates a TextAdornment1 manager when a textView is created.
        /// </summary>
        /// <param name="textView">The <see cref="IWpfTextView"/> upon which the adornment should be placed</param>
        public void TextViewCreated(IWpfTextView textView)
        {
            _view = textView;
            dte2 = (DTE2)ServiceProvider.GetService(typeof(DTE));
            new TextAdornment1(textView);
        }
    }
    #endregion //Adornment Factory
}

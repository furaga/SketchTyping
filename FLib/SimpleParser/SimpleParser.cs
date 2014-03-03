using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Drawing;
using System.Text.RegularExpressions;

namespace FLib
{
    public class SimpleParser
    {
        public static readonly Regex CALL_FUNCTION = new Regex(
            @"((?<Name>[\w]+))\s*\((?<Arguments>[^\;]*)\;"
            , RegexOptions.Compiled);

        public static readonly Regex CLASS_DECL = new Regex(
            "(^|;)\\s*(?<Accessor>(public|private|protected|final|static|abstract)\\s+)*" +
            "class" +
            "(\\s*\\[\\s*\\])?\\s+(?<ClassName>[a-zA-Z0-9]+)(?<Extension>[^{]*){",
            RegexOptions.Compiled | RegexOptions.Multiline);

        public static readonly Regex FUNCTION_DECL = new Regex(FUNCTION_DECL_STR,
            RegexOptions.Compiled | RegexOptions.Multiline);

        public const string FUNCTION_DECL_STR =
            "(^|}|;)\\s*(?<Accessor>(unsafe|public|private|protected|final|static)\\s+)*" +
            "(?<ReturnType>\\w+)?" +
            "(\\s*\\[\\s*\\])?\\s+(?<FunctionName>[a-zA-Z0-9]+)\\s*\\((?<Arguments>[^\\)]*)\\)\\s*{";
    }
}

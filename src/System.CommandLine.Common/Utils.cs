using System;
using System.Collections.Generic;
using System.IO;
using System.Reflection;
using System.Text;

namespace System.CommandLine.Common
{
    public class ParserUtils
    {
        public static readonly Lazy<string> ExeName =
                new Lazy<string>(() => Path.GetFileNameWithoutExtension(Assembly.GetEntryAssembly().Location));

    }
}

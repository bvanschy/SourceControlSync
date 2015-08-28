using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Web;

namespace SourceControlSync.WebApi.Util
{
    public class Logger : ILogger
    {
        public void TraceInformation(string format, params object[] args)
        {
            Trace.TraceInformation(format, args);
        }
    }
}
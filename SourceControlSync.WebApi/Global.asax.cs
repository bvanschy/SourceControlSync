using Microsoft.Azure;
using SourceControlSync.WebApi.TraceListeners;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Web;
using System.Web.Http;
using System.Web.Routing;

namespace SourceControlSync.WebApi
{
    public class WebApiApplication : System.Web.HttpApplication
    {
        protected void Application_Start()
        {
            InitializeTraceListeners();
            GlobalConfiguration.Configure(WebApiConfig.Register);
        }

        private static void InitializeTraceListeners()
        {
            Trace.AutoFlush = false;
            Trace.IndentSize = 0;

            Trace.Listeners.Clear();

            var smtpParameters = CloudConfigurationManager.GetSetting("SMTPParameters");
            var smtpTraceListener = new SmtpTraceListener(smtpParameters);
            Trace.Listeners.Add(smtpTraceListener);
        }
    }
}

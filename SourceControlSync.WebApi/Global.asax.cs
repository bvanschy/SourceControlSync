using Microsoft.Azure;
using Microsoft.Practices.Unity;
using SourceControlSync.WebApi.App_Start;
using System.Diagnostics;
using System.Web.Http;

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
            var traceListenerParameters = CloudConfigurationManager.GetSetting("TraceListenerParameters");
            if (!string.IsNullOrWhiteSpace(traceListenerParameters))
            {
                Trace.AutoFlush = false;
                Trace.IndentSize = 0;

                Trace.Listeners.Clear();

                var unityContainer = UnityConfig.GetConfiguredContainer();
                var smtpTraceListener = unityContainer.Resolve<TraceListener>(
                    new ParameterOverride("initializeData", traceListenerParameters));
                Trace.Listeners.Add(smtpTraceListener);
            }
        }
    }
}

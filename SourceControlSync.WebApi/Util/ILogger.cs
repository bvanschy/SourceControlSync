using System;
namespace SourceControlSync.WebApi.Util
{
    public interface ILogger
    {
        void TraceInformation(string format, params object[] args);
    }
}

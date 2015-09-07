using System.Collections.Generic;

namespace SourceControlSync.Domain
{
    public interface IChangesReport : IErrorReport
    {
        IExecutedCommands ExecutedCommands { get; set; }
    }
}

using System.Collections.Generic;

namespace SourceControlSync.Domain
{
    public interface IChangesReport : IErrorReport
    {
        IList<ChangeCommandPair> ExecutedCommands { get; set; }
    }
}

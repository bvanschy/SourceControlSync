using System;

namespace SourceControlSync.Domain
{
    public interface IClock
    {
        DateTime UtcNow { get; }
    }
}

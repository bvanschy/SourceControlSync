using SourceControlSync.Domain.Models;
using System;
using System.Collections.Generic;

namespace SourceControlSync.Domain
{
    public interface IChangesCalculator
    {
        IEnumerable<ItemChange> ItemChanges { get; }
        void CalculateItemChanges(IEnumerable<Commit> commits);
    }
}

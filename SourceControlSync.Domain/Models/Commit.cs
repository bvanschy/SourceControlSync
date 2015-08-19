using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace SourceControlSync.Domain.Models
{
    public class Commit
    {
        public IEnumerable<ItemChange> Changes { get; set; }
        public string CommitId { get; set; }
        public UserDate Committer { get; set; }
    }
}

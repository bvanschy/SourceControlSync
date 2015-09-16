using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace SourceControlSync.Domain.Models
{
    public class Commit
    {
        private IList<ItemChange> _changes;

        public Commit(string commitId, UserDate committer)
        {
            CommitId = commitId;
            Committer = committer;
        }

        public string CommitId { get; private set; }

        public UserDate Committer { get; private set; }

        public IEnumerable<ItemChange> Changes 
        {
            get { return _changes; }
            set 
            {
                if(_changes != null)
                {
                    throw new InvalidOperationException("Changes is immutable");
                }
                if (value != null)
                {
                    _changes = value.ToList();
                }
            }
        }
    }
}

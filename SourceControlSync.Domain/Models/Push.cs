using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SourceControlSync.Domain.Models
{
    public class Push
    {
        public IEnumerable<Commit> Commits { get; set; }
        public Repository Repository { get; set; }
    }
}

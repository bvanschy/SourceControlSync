using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace SourceControlSync.Domain.Models
{
    public class Repository
    {
        public Repository(Guid id)
        {
            Id = id;
        }

        public Guid Id { get; private set; }
    }
}

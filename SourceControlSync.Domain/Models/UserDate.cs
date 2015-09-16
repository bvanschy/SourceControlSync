using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace SourceControlSync.Domain.Models
{
    public class UserDate
    {
        public UserDate(DateTime date)
        {
            Date = date;
        }

        public DateTime Date { get; private set; }
    }
}

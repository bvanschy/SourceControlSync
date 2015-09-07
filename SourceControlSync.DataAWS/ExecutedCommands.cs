using SourceControlSync.Domain;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SourceControlSync.DataAWS
{
    public class ExecutedCommands : IExecutedCommands
    {
        private readonly IList<IItemCommand> _executedCommands;

        public ExecutedCommands(IEnumerable<IItemCommand> executedCommands)
        {
            _executedCommands = executedCommands.ToList();
        }

        public override string ToString()
        {
            var summary = new StringBuilder();
            foreach (var command in _executedCommands.SelectMany(c => c.GetDescription("{0}\t{1}")).OrderBy(d => d))
            {
                summary.AppendLine(command);
            }
            return summary.ToString();
        }
    }
}

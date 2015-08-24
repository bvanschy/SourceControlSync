using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SourceControlSync.DataAWS
{
    public interface ICommandFactory
    {
        IItemCommand CreateUploadCommand();
        IItemCommand CreateDeleteCommand();
        IItemCommand CreateNullCommand();
    }
}

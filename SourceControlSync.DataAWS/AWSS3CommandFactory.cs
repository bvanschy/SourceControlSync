using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SourceControlSync.DataAWS
{
    public class AWSS3CommandFactory : ICommandFactory
    {
        public IItemCommand CreateUploadCommand()
        {
            return new UploadItemCommand();
        }

        public IItemCommand CreateDeleteCommand()
        {
            return new DeleteItemCommand();
        }

        public IItemCommand CreateNullCommand()
        {
            return new NullItemCommand();
        }
    }
}

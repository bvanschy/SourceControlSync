using Amazon;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SourceControlSync.DataAWS
{
    public class Bucket
    {
        public RegionEndpoint Region { get; set; }
        public string RegionSystemName
        {
            get
            {
                return Region.SystemName;
            }
            set
            {
                Region = RegionEndpoint.GetBySystemName(value);
            }
        }
        public string BucketName { get; set; }
    }
}

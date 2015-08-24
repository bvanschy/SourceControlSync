using SourceControlSync.Domain;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SourceControlSync.DataAWS
{
    public class AWSS3ConnectionStringBuilder : ConnectionStringBuilder
    {
        private const string PROPERTY_BUCKETNAME = "BucketName";
        private const string PROPERTY_REGIONSYSTEMNAME = "RegionSystemName";
        private const string PROPERTY_ACCESSKEYID = "AccessKeyId";
        private const string PROPERTY_SECRETACCESSKEY = "SecretAccessKey";

        public AWSS3ConnectionStringBuilder(string connectionString)
        {
            ConnectionString = connectionString;
        }

        public AWSS3ConnectionStringBuilder()
        {
        }

        public Bucket Bucket 
        {
            get 
            {
                return new Bucket()
                {
                    BucketName = GetValue(PROPERTY_BUCKETNAME),
                    RegionSystemName = GetValue(PROPERTY_REGIONSYSTEMNAME)
                };
            }
            set 
            {
                SetValue(PROPERTY_BUCKETNAME, value.BucketName);
                SetValue(PROPERTY_REGIONSYSTEMNAME, value.RegionSystemName);
            }
        }

        public Credentials Credentials 
        { 
            get
            {
                return new Credentials()
                {
                    AccessKeyId = GetValue(PROPERTY_ACCESSKEYID),
                    SecretAccessKey = GetValue(PROPERTY_SECRETACCESSKEY)
                };
            }
            set
            {
                SetValue(PROPERTY_ACCESSKEYID, value.AccessKeyId);
                SetValue(PROPERTY_SECRETACCESSKEY, value.SecretAccessKey);
            }
        }
    }
}

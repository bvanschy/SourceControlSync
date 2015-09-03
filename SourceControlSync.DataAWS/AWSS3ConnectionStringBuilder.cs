using SourceControlSync.Domain;

namespace SourceControlSync.DataAWS
{
    public class AWSS3ConnectionStringBuilder : ConnectionStringBuilder
    {
        public const string PROPERTY_BUCKETNAME = "BucketName";
        public const string PROPERTY_REGIONSYSTEMNAME = "RegionSystemName";
        public const string PROPERTY_ACCESSKEYID = "AccessKeyId";
        public const string PROPERTY_SECRETACCESSKEY = "SecretAccessKey";

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

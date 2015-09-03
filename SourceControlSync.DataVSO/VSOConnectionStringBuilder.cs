using SourceControlSync.Domain;
using System;

namespace SourceControlSync.DataVSO
{
    public class VSOConnectionStringBuilder : ConnectionStringBuilder
    {
        public const string PROPERTY_BASEURL = "BaseUrl";
        public const string PROPERTY_USERNAME = "UserName";
        public const string PROPERTY_ACCESSTOKEN = "AccessToken";

        public VSOConnectionStringBuilder(string connectionString)
        {
            ConnectionString = connectionString;
        }

        public VSOConnectionStringBuilder()
        {
        }

        public Uri BaseUrl
        {
            get
            {
                var baseUrl = GetValue(PROPERTY_BASEURL);
                return baseUrl != null ? new Uri(baseUrl) : null;
            }
            set
            {
                SetValue(PROPERTY_BASEURL, value.OriginalString);
            }
        }

        public Credentials Credentials
        {
            get
            {
                return new Credentials()
                {
                    UserName = GetValue(PROPERTY_USERNAME),
                    AccessToken = GetValue(PROPERTY_ACCESSTOKEN)
                };
            }
            set
            {
                SetValue(PROPERTY_USERNAME, value.UserName);
                SetValue(PROPERTY_ACCESSTOKEN, value.AccessToken);
            }
        }
    }
}

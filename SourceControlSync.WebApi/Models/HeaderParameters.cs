using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http.Headers;
using System.Web;

namespace SourceControlSync.WebApi.Models
{
    public class HeaderParameters
    {
        private IDictionary<string, string> _headerValues;

        public HeaderParameters(HttpRequestHeaders headers, params string[] names)
        {
            _headerValues = names.ToDictionary(
                name => name, 
                name => GetHeaderValue(headers, name));
        }

        public string this[string name]
        {
            get
            {
                if (_headerValues.ContainsKey(name))
                    return _headerValues[name];
                else
                    return null;
            }
        }

        public bool AnyMissing
        {
            get
            {
                return _headerValues.Values.Any(value => value == null);
            }
        }

        private static string GetHeaderValue(HttpRequestHeaders headers, string headerName)
        {
            IEnumerable<string> values;
            if (headers.TryGetValues(headerName, out values))
            {
                return values.FirstOrDefault();
            }
            return null;
        }
    }
}
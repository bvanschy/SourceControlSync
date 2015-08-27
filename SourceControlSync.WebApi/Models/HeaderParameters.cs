using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http.Headers;
using System.Web;

namespace SourceControlSync.WebApi.Models
{
    public class HeaderParameters
    {
        private readonly HttpRequestHeaders _headers;
        private readonly IEnumerable<string> _headerNames;

        public HeaderParameters(HttpRequestHeaders headers, params string[] names)
        {
            _headers = headers;
            _headerNames = names.ToArray();
        }

        public string this[string name]
        {
            get
            {
                return GetValue(name);
            }
        }

        public bool AnyMissing
        {
            get
            {
                return _headerNames.Any(name => !_headers.Contains(name));
            }
        }

        private string GetValue(string headerName)
        {
            var values = _headers.GetValues(headerName);
            return values.FirstOrDefault();
        }
    }
}
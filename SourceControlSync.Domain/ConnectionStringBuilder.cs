using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SourceControlSync.Domain
{
    public class ConnectionStringBuilder
    {
        private IDictionary<string, string> _values;

        protected ConnectionStringBuilder()
        {
        }

        public string ConnectionString
        {
            get
            {
                if (_values != null && _values.Any())
                {
                    return GetConnectionString();
                }
                return null;
            }
            set
            {
                if (!string.IsNullOrWhiteSpace(value))
                {
                    SetConnectionString(value);
                }
                else
                {
                    _values = null;
                }
            }
        }

        private string GetConnectionString()
        {
            var pairs = _values.Select(keyValue => string.Format("{0}={1}", keyValue.Key, keyValue.Value));
            return string.Join(";", pairs);
        }

        private void SetConnectionString(string value)
        {
            var pairs = value.Split(';');
            _values = pairs.ToDictionary(
                pair => pair.Substring(0, pair.IndexOf('=')),
                pair => pair.Substring(pair.IndexOf('=') + 1));
        }

        protected string GetValue(string key)
        {
            if (_values != null && _values.ContainsKey(key))
            {
                return _values[key];
            }
            return null;
        }

        protected void SetValue(string key, string value)
        {
            SetupDictionary();
            if (_values.ContainsKey(key))
            {
                _values[key] = value;
            }
            else
            {
                _values.Add(key, value);
            }
        }

        private void SetupDictionary()
        {
            if (_values == null)
            {
                _values = new Dictionary<string, string>();
            }
        }

    }
}

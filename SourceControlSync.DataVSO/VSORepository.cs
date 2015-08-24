using SourceControlSync.Domain;
using SourceControlSync.Domain.Models;
using System;
using System.Net;
using System.Threading;
using System.Threading.Tasks;

namespace SourceControlSync.DataVSO
{
    public class VSORepository : ISourceRepository
    {
        private Uri _baseUrl;
        private Credentials _credentials;

        public string ConnectionString
        {
            set
            {
                var connectionStringBuilder = new VSOConnectionStringBuilder(value);
                _baseUrl = connectionStringBuilder.BaseUrl;
                _credentials = connectionStringBuilder.Credentials;
            }
        }

        public async Task DownloadChangesAsync(Push push, string root, CancellationToken token)
        {
            ValidateConnectionParameters();
            var httpClient = CreateHttpClient();
            var request = new DownloadRequest(httpClient);
            await request.DownloadChangesInAllCommitsAsync(push, root, token);
        }

        private void ValidateConnectionParameters()
        {
            if (_baseUrl == null)
            {
                throw new ApplicationException("Invalid ConnectionString");
            }
            if (_credentials == null ||
                string.IsNullOrWhiteSpace(_credentials.UserName) ||
                string.IsNullOrWhiteSpace(_credentials.AccessToken))
            {
                throw new ApplicationException("Invalid ConnectionString");
            }
        }

        private Microsoft.TeamFoundation.SourceControl.WebApi.GitHttpClient CreateHttpClient()
        {
            var vssCredentials = new Microsoft.VisualStudio.Services.Common.VssCredentials(
                new Microsoft.VisualStudio.Services.Common.WindowsCredential(
                    new NetworkCredential(_credentials.UserName, _credentials.AccessToken)));
            var httpClient = new Microsoft.TeamFoundation.SourceControl.WebApi.GitHttpClient(_baseUrl, vssCredentials);
            return httpClient;
        }
    }
}

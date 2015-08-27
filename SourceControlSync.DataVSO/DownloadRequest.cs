using SourceControlSync.Domain;
using SourceControlSync.Domain.Extensions;
using SourceControlSync.Domain.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace SourceControlSync.DataVSO
{
    public class DownloadRequest : IDownloadRequest
    {
        private readonly Uri _baseUrl;
        private readonly Credentials _credentials;

        private Microsoft.TeamFoundation.SourceControl.WebApi.GitHttpClient _httpClient;

        public DownloadRequest(string connectionString)
        {
            var connectionStringBuilder = new VSOConnectionStringBuilder(connectionString);
            _baseUrl = connectionStringBuilder.BaseUrl;
            _credentials = connectionStringBuilder.Credentials;
        }

        public async Task DownloadChangesInCommitAsync(Commit commit, Guid repositoryId, CancellationToken token)
        {
            CreateHttpClient();
            var commitChanges = await _httpClient.GetChangesAsync(commit.CommitId, repositoryId,
                                        cancellationToken: token);
            commit.Changes = commitChanges.Changes.ToSync();
        }

        public async Task DownloadItemAndContentInCommitAsync(ItemChange change, Commit commit, Guid repositoryId, CancellationToken token)
        {
            CreateHttpClient();

            var item = await _httpClient.GetItemAsync(repositoryId, change.Item.Path,
                                includeContentMetadata: true,
                                versionDescriptor: new Microsoft.TeamFoundation.SourceControl.WebApi.GitVersionDescriptor()
                                {
                                    VersionType = Microsoft.TeamFoundation.SourceControl.WebApi.GitVersionType.Commit,
                                    Version = commit.CommitId
                                },
                                cancellationToken: token);

            var content = await _httpClient.GetBlobContentAsync(repositoryId, item.ObjectId,
                                cancellationToken: token);

            change.Item.ContentMetadata = item.ContentMetadata.ToSync();
            await change.SetNewContentAsync(content, token);
        }

        private void CreateHttpClient()
        {
            if (_httpClient == null)
            {
                ValidateConnectionParameters();
                var vssCredentials = new Microsoft.VisualStudio.Services.Common.VssCredentials(
                    new Microsoft.VisualStudio.Services.Common.WindowsCredential(
                        new NetworkCredential(_credentials.UserName, _credentials.AccessToken)));
                _httpClient = new Microsoft.TeamFoundation.SourceControl.WebApi.GitHttpClient(_baseUrl, vssCredentials);
            }
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

        public void Dispose()
        {
            _httpClient = null;
        }
    }
}

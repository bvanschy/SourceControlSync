﻿using SourceControlSync.Domain;
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
    /// <summary>
    /// File changes downloader for Visual Studio Team Services Git repository
    /// </summary>
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

        public DownloadRequest(string baseUrl, Credentials credentials)
        {
            _baseUrl = new Uri(baseUrl);
            _credentials = credentials;
        }

        public async Task<IEnumerable<ItemChange>> DownloadChangesInCommitAsync(string commitId, Guid repositoryId, CancellationToken token)
        {
            CreateHttpClient();
            var commitChanges = await _httpClient.GetChangesAsync(
                commitId,
                repositoryId,
                cancellationToken: token
                );
            return commitChanges.Changes.ToSync();
        }

        public async Task DownloadItemAndContentInCommitAsync(ItemChange change, string commitId, Guid repositoryId, CancellationToken token)
        {
            CreateHttpClient();

            var item = await _httpClient.GetItemAsync(
                repositoryId,
                change.Item.Path,
                includeContentMetadata: true,
                versionDescriptor: new Microsoft.TeamFoundation.SourceControl.WebApi.GitVersionDescriptor()
                {
                    VersionType = Microsoft.TeamFoundation.SourceControl.WebApi.GitVersionType.Commit,
                    Version = commitId
                },
                cancellationToken: token
                );

            change.Item.ContentMetadata = item.ContentMetadata.ToSync();

            if (!item.IsFolder)
            {
                var content = await _httpClient.GetBlobContentAsync(repositoryId, item.ObjectId, cancellationToken: token);
                await change.SetNewContentAsync(content, token);
            }
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
    }
}

using Microsoft.VisualStudio.Services.Common;
using SourceControlSync.Domain;
using SourceControlSync.Domain.Models;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading.Tasks;

namespace SourceControlSync.DataVSO
{
    public class VSORepository : ISourceRepository
    {
        private string _baseUrl;
        private string _userName;
        private string _accessToken;

        public VSORepository(string baseUrl, string userName, string accessToken)
        {
            _baseUrl = baseUrl;
            _userName = userName;
            _accessToken = accessToken;
        }

        public async Task DownloadChangesAsync(Push push, string root = null)
        {
            var repositoryId = push.Repository.Id;
            var credentials = new VssCredentials(new WindowsCredential(new NetworkCredential(_userName, _accessToken)));
            var httpClient = new Microsoft.TeamFoundation.SourceControl.WebApi.GitHttpClient(new Uri(_baseUrl), credentials);

            var itemTasks = new List<Task>();
            foreach (var commit in push.Commits)
            {
                var commitChanges = await httpClient.GetChangesAsync(commit.CommitId, repositoryId);
                var itemChanges = commitChanges.Changes;
                commit.Changes = itemChanges.Where(c => root == null || c.Item.Path.StartsWith(root)).ToSync();

                foreach (var change in commit.Changes)
                {
                    if ((change.ChangeType & (ItemChangeType.Edit | ItemChangeType.Add | ItemChangeType.Rename)) != 0)
                    {
                        itemTasks.Add(DownloadItemAsync(repositoryId, commit.CommitId, change, httpClient));
                    }
                }
            }
            await Task.WhenAll(itemTasks);
        }

        private static async Task DownloadItemAsync(Guid repositoryId, string commitId, ItemChange change,
            Microsoft.TeamFoundation.SourceControl.WebApi.GitHttpClient httpClient)
        {
            var item = await httpClient.GetItemAsync(repositoryId, change.Item.Path, includeContentMetadata: true,
                                                        versionDescriptor: new Microsoft.TeamFoundation.SourceControl.WebApi.GitVersionDescriptor() 
                                                        { 
                                                            VersionType = Microsoft.TeamFoundation.SourceControl.WebApi.GitVersionType.Commit, 
                                                            Version = commitId 
                                                        });
            var content = await httpClient.GetBlobContentAsync(repositoryId, item.ObjectId);

            change.Item.ContentMetadata = item.ContentMetadata.ToSync();

            ItemContent itemContent = null;
            if (change.Item.ContentMetadata.IsBinary)
            {
                itemContent = await CreateEncodedBinaryItemContent(content);
            }
            else
            {
                itemContent = await CreateRawTextItemContent(change.Item.ContentMetadata.Encoding, content);
            }
            change.NewContent = itemContent;
        }

        private static async Task<ItemContent> CreateEncodedBinaryItemContent(Stream content)
        {
            var itemContent = new ItemContent();
            itemContent.ContentType = ItemContentType.Base64Encoded;
            using (var memoryStream = new MemoryStream())
            {
                await content.CopyToAsync(memoryStream);
                itemContent.Content = Convert.ToBase64String(memoryStream.ToArray());
            }
            return itemContent;
        }

        private static async Task<ItemContent> CreateRawTextItemContent(Encoding encoding, Stream content)
        {
            var itemContent = new ItemContent();
            using (var streamReader = new StreamReader(content, encoding))
            {
                itemContent.ContentType = ItemContentType.RawText;
                itemContent.Content = await streamReader.ReadToEndAsync();
            }
            return itemContent;
        }

    }
}

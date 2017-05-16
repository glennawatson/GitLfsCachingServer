// --------------------------------------------------------------------------------------------------------------------
// <copyright file="FileCachingLfsClient.cs" company="Glenn Watson">
//     Copyright (C) 2017. Glenn Watson
// </copyright>
// --------------------------------------------------------------------------------------------------------------------

namespace GitLfs.Client
{
    using System;
    using System.Collections.Generic;
    using System.IO;
    using System.Linq;
    using System.Net.Http;
    using System.Net.Http.Headers;
    using System.Text;
    using System.Threading.Tasks;

    using GitLfs.Core;
    using GitLfs.Core.BatchRequest;
    using GitLfs.Core.BatchResponse;
    using GitLfs.Core.Managers;

    public class FileCachingLfsClient : ILfsClient
    {
        private readonly IFileManager fileManager;

        private readonly IRequestSerialiser requestSerialiser;

        private readonly ITransferSerialiser transferSerialiser;

        public FileCachingLfsClient(
            IFileManager fileManager,
            IRequestSerialiser requestSerialiser,
            ITransferSerialiser transferSerialiser)
        {
            this.fileManager = fileManager;
            this.requestSerialiser = requestSerialiser;
            this.transferSerialiser = transferSerialiser;
        }

        /// <inheritdoc />
        public async Task<Stream> DownloadFile(GitHost host, string repositoryName, RequestObject requestObject)
        {
            using (var httpClient = this.CreateBatchClient(host))
            {
                var request = new Request
                                  {
                                      Objects = new List<RequestObject> { requestObject },
                                      Operation = RequestMode.Download,
                                      Transfers = new List<TransferMode> { TransferMode.Basic }
                                  };

                var url = new Uri($"{host.Href}/{repositoryName}/info/lfs/objects/batch");
                HttpResponseMessage result = await this.PostBatchRequest(httpClient, url, request);

                if (result.IsSuccessStatusCode == false)
                {
                    throw new ClientDownloadException(result.StatusCode, result.ReasonPhrase);
                }

                Transfer transfer = this.transferSerialiser.FromString(await result.Content.ReadAsStringAsync());

                if (transfer.Objects.Count != 1)
                {
                    throw new Exception("Got the wrong number of objects back from the server.");
                }

                BatchObjectBase objectValue = transfer.Objects.Single();

                BatchObjectError error = objectValue as BatchObjectError;

                if (error != null)
                {
                    throw new ClientDownloadException(error.ErrorCode, error.ErrorMessage);    
                }

                BatchObject batchObjectFile = objectValue as BatchObject;

                if (batchObjectFile == null)
                {
                    throw new ClientDownloadException(404, "We got a invalid batch object back");
                }

                BatchObjectAction action = batchObjectFile.Actions.Single();
                using (var downloadHttpClient = new HttpClient())
                {
                    foreach (KeyValuePair<string, string> header in action.Headers)
                    {
                        downloadHttpClient.DefaultRequestHeaders.Add(header.Key, header.Value);
                    }

                    Stream stream = await downloadHttpClient.GetStreamAsync(action.HRef);
                    return new FileStream(
                        await this.fileManager.SaveFileForObjectId(repositoryName, requestObject.ObjectId, stream),
                        FileMode.Open,
                        FileAccess.Read);
                }
            }
        }

        private async Task<HttpResponseMessage> PostBatchRequest(HttpClient httpClient, Uri url, Request request)
        {
            HttpResponseMessage result = await httpClient.PostAsync(
                                             url,
                                             new StringContent(
                                                 this.requestSerialiser.ToString(request),
                                                 null,
                                                 "application/vnd.git-lfs+json"));
            return result;
        }

        /// <inheritdoc />
        public async Task UploadFile(GitHost host, string repositoryName, string objectId)
        {

        }

        private HttpClient CreateBatchClient(GitHost host)
        {
            HttpClient client = new HttpClient();
            client.DefaultRequestHeaders.Accept.Clear();
            client.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("application/vnd.git-lfs+json"));
            var authValue = new AuthenticationHeaderValue("token", host.Token);
            client.DefaultRequestHeaders.Authorization = authValue;
            return client;
        }
    }
}
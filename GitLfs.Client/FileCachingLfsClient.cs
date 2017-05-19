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
    using System.Threading.Tasks;

    using GitLfs.Core;
    using GitLfs.Core.BatchRequest;
    using GitLfs.Core.BatchResponse;
    using GitLfs.Core.Managers;

    using Microsoft.Extensions.Logging;

    public class FileCachingLfsClient : ILfsClient
    {
        private readonly IFileManager fileManager;

        private readonly IBatchRequestSerialiser requestSerialiser;

        private readonly IBatchTransferSerialiser transferSerialiser;

        private readonly ILogger logger;

        public FileCachingLfsClient(
            IFileManager fileManager,
            IBatchRequestSerialiser requestSerialiser,
            IBatchTransferSerialiser transferSerialiser,
            ILogger<FileCachingLfsClient> logger)
        {
            this.fileManager = fileManager;
            this.requestSerialiser = requestSerialiser;
            this.transferSerialiser = transferSerialiser;
            this.logger = logger;
        }

        /// <inheritdoc />
        public async Task<Stream> DownloadFile(GitHost host, string repositoryName, BatchRequestObject requestObject)
        {
            IEnumerable<BatchObjectAction> actions = await this.SendBatchRequest(host, repositoryName, requestObject, BatchRequestMode.Download);
            BatchObjectAction action = actions.SingleOrDefault();

            using (var httpClient = new HttpClient())
            {
                SetClientHeaders(action, httpClient);

                Stream stream = await httpClient.GetStreamAsync(action.HRef);
                return new FileStream(
                    await this.fileManager.SaveFileForObjectId(repositoryName, requestObject.ObjectId, stream),
                    FileMode.Open,
                    FileAccess.Read);
            }
        }

        /// <inheritdoc />
        public async Task UploadFile(GitHost host, string repositoryName, BatchRequestObject requestObject)
        {
            IEnumerable<BatchObjectAction> action = await this.SendBatchRequest(host, repositoryName, requestObject, BatchRequestMode.Upload);

            using (var httpClient = new HttpClient())
            {
                SetClientHeaders(action, httpClient);

                Stream stream = this.fileManager.GetFileForObjectId(repositoryName, requestObject.ObjectId);

                using (var content = new StreamContent(stream))
                {
                    content.Headers.ContentType = MediaTypeHeaderValue.Parse("application/octet-stream");
                    logger.LogInformation($"Uploading to {action.HRef} with repository name {repositoryName}, request:{requestObject.ObjectId.Substring(0, 10)}/{requestObject.Size}");
                    var result = await httpClient.PutAsync(action.HRef, content);

                    if (result.IsSuccessStatusCode == false)
                    {
                        logger.LogWarning($"Failed to download request:{requestObject.ObjectId.Substring(0, 10)}/{requestObject.Size}");
                        throw new ClientException(result.StatusCode, result.ReasonPhrase);
                    }
                }
            }
        }

        private static void SetClientHeaders(BatchObjectAction action, HttpClient downloadHttpClient)
        {
            if (action.Headers != null)
            {
                foreach (KeyValuePair<string, string> header in action.Headers)
                {
                    downloadHttpClient.DefaultRequestHeaders.Add(header.Key, header.Value);
                }
            }
        }

        private static Uri GetLfsBatchUrl(GitHost host, string repositoryName)
        {
            var url = new Uri($"{host.Href}/{repositoryName}/info/lfs/objects/batch");
            return url;
        }

        private async Task<IEnumerable<BatchObjectAction>> SendBatchRequest(GitHost host, string repositoryName, BatchRequestObject requestObject, BatchRequestMode requestMode)
        {
            using (HttpClient client = new HttpClient())
            {
                client.DefaultRequestHeaders.Accept.Clear();
                client.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("application/vnd.git-lfs+json"));
                var authValue = new AuthenticationHeaderValue("token", host.Token);
                client.DefaultRequestHeaders.Authorization = authValue;

                var request = new BatchRequest
                {
                    Objects = new List<BatchRequestObject> { requestObject },
                    Operation = requestMode,
                    Transfers = new List<TransferMode> { TransferMode.Basic }
                };

                using (var content = new StringContent(
                    this.requestSerialiser.ToString(request),
                    null,
                    "application/vnd.git-lfs+json"))
                {
                    HttpResponseMessage result = await client.PostAsync(GetLfsBatchUrl(host, repositoryName), content);

                    if (result.IsSuccessStatusCode == false)
                    {
                        throw new ClientException(result.StatusCode, result.ReasonPhrase);
                    }

                    BatchTransfer transfer = this.transferSerialiser.FromString(await result.Content.ReadAsStringAsync());

                    if (transfer.Objects.Count != 1)
                    {
                        throw new ClientException(500, "Got the wrong number of objects back from the server.");
                    }

                    BatchObjectBase objectValue = transfer.Objects.Single();

                    BatchObjectError error = objectValue as BatchObjectError;

                    if (error != null)
                    {
                        throw new ClientException(error.ErrorCode, error.ErrorMessage);
                    }

                    BatchObject batchObjectFile = objectValue as BatchObject;

                    if (batchObjectFile == null)
                    {
                        throw new ClientException(404, "We got a invalid batch object back");
                    }

                    return batchObjectFile.Actions;
                }
            }
        }
    }
}
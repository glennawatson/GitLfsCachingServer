// <copyright file="FileCachingLfsClient.cs" company="Glenn Watson">
//    Copyright (C) 2017. Glenn Watson
// </copyright>

namespace GitLfs.Client
{
    using System;
    using System.IO;
    using System.Net.Http;
    using System.Net.Http.Headers;
    using System.Threading.Tasks;

    using GitLfs.Core;
    using GitLfs.Core.BatchRequest;
    using GitLfs.Core.BatchResponse;
    using GitLfs.Core.Error;
    using GitLfs.Core.File;
    using GitLfs.Core.Verify;

    using Microsoft.Extensions.Logging;

    public class FileCachingLfsClient : ILfsClient
    {
        private readonly IErrorResponseSerialiser errorResponseSerialiser;

        private readonly IFileManager fileManager;

        private readonly ILogger logger;

        private readonly IBatchRequestSerialiser requestSerialiser;

        private readonly IBatchTransferSerialiser transferSerialiser;

        private readonly IVerifyObjectSerialiser verifySerialiser;

        public FileCachingLfsClient(
            IFileManager fileManager,
            IBatchRequestSerialiser requestSerialiser,
            IBatchTransferSerialiser transferSerialiser,
            IVerifyObjectSerialiser verifySerialiser,
            IErrorResponseSerialiser errorResponseSerialiser,
            ILogger<FileCachingLfsClient> logger)
        {
            this.fileManager = fileManager;
            this.requestSerialiser = requestSerialiser;
            this.transferSerialiser = transferSerialiser;
            this.verifySerialiser = verifySerialiser;
            this.errorResponseSerialiser = errorResponseSerialiser;
            this.logger = logger;
        }

        public async Task<Stream> DownloadFile(
            GitHost host,
            string repositoryName,
            ObjectId objectId,
            BatchObjectAction action)
        {
            using (var httpClient = new HttpClient())
            {
                SetClientHeaders(action, httpClient);

                this.logger.LogInformation(
                    $"Download from {action.HRef} with repository name {repositoryName}, request:{objectId.Hash.Substring(0, 10)}/{objectId.Size}");
                HttpResponseMessage result = await httpClient.GetAsync(action.HRef);

                if (result.IsSuccessStatusCode == false)
                {
                    await this.HandleError(result);
                }

                this.logger.LogInformation(
                    $"Now saving to a file {repositoryName}, request:{objectId.Hash.Substring(0, 10)}/{objectId.Size}\"");
                string fileName = this.fileManager.SaveFile(
                    repositoryName,
                    objectId,
                    FileLocation.Permenant,
                    await result.Content.ReadAsStreamAsync());
                this.logger.LogInformation(
                    $"Saved to file {repositoryName}, request:{objectId.Hash.Substring(0, 10)}/{objectId.Size}\"");

                return new FileStream(fileName, FileMode.Open, FileAccess.Read);
            }
        }

        /// <inheritdoc />
        public async Task<BatchTransfer> RequestBatch(GitHost host, string repositoryName, BatchRequest request)
        {
            using (var client = new HttpClient())
            {
                client.DefaultRequestHeaders.Accept.Clear();
                client.DefaultRequestHeaders.Accept.Add(
                    new MediaTypeWithQualityHeaderValue("application/vnd.git-lfs+json"));
                var authValue = new AuthenticationHeaderValue("token", host.Token);
                client.DefaultRequestHeaders.Authorization = authValue;

                using (var content = new StringContent(
                    this.requestSerialiser.ToString(request),
                    null,
                    "application/vnd.git-lfs+json"))
                {
                    HttpResponseMessage result = await client.PostAsync(GetLfsBatchUrl(host, repositoryName), content);

                    if (result.IsSuccessStatusCode == false)
                    {
                        await this.HandleError(result);
                    }

                    string returnContents = await result.Content.ReadAsStringAsync();
                    BatchTransfer transfer = this.transferSerialiser.TransferFromString(returnContents);

                    return transfer;
                }
            }
        }

        public async Task UploadFile(BatchObjectAction action, Stream stream)
        {
            using (var httpClient = new HttpClient())
            {
                SetClientHeaders(action, httpClient);

                var content = new StreamContent(stream, 2000);
                content.Headers.ContentType = MediaTypeHeaderValue.Parse("application/octet-stream");
                this.logger.LogInformation($"Uploading to {action.HRef}");
                HttpResponseMessage result = await httpClient.PutAsync(action.HRef, content);

                if (result.IsSuccessStatusCode == false)
                {
                    this.logger.LogInformation($"Failed uploading to {action.HRef}");
                    await this.HandleError(result);
                }
                else
                {
                    this.logger.LogInformation(
                        $"Success uploading to {action.HRef}");
                }
            }
        }

        public async Task Verify(GitHost host, string repositoryName, ObjectId objectId, BatchObjectAction action)
        {
            using (var httpClient = new HttpClient())
            {
                SetClientHeaders(action, httpClient);

                using (var content = new StringContent(
                    this.verifySerialiser.ToString(objectId),
                    null,
                    "application/vnd.git-lfs+json"))
                {
                    this.logger.LogInformation(
                        $"Verify from {action.HRef} with repository name {repositoryName}, request:{objectId.Hash.Substring(0, 10)}/{objectId.Size}");
                    HttpResponseMessage result = await httpClient.PostAsync(action.HRef, content);

                    if (result.IsSuccessStatusCode == false)
                    {
                        await this.HandleError(result);
                    }
                }
            }
        }

        private static Uri GetLfsBatchUrl(GitHost host, string repositoryName)
        {
            var url = new Uri($"{host.Href}/{repositoryName}/info/lfs/objects/batch");
            return url;
        }

        private static void SetClientHeaders(BatchObjectAction action, HttpClient downloadHttpClient)
        {
            if (action.Headers != null)
            {
                foreach (BatchHeader header in action.Headers)
                {
                    downloadHttpClient.DefaultRequestHeaders.TryAddWithoutValidation(header.Key, header.Value);
                }
            }

            downloadHttpClient.DefaultRequestHeaders.Add("User-Agent", "glennawatson");
        }

        private async Task HandleError(HttpResponseMessage result)
        {
            ErrorResponse errorResponse;
            try
            {
                errorResponse = this.errorResponseSerialiser.FromString(await result.Content.ReadAsStringAsync());
            }
            catch (ParseException)
            {
                errorResponse = new ErrorResponse { Message = result.ReasonPhrase };
            }

            this.logger.LogWarning($"Operation failed. {errorResponse}");

            throw new ErrorResponseException(errorResponse, (int)result.StatusCode);
        }
    }
}
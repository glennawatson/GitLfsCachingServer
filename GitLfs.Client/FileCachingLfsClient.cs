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
        private readonly IFileManager fileManager;

        private readonly IBatchRequestSerialiser requestSerialiser;

        private readonly IBatchTransferSerialiser transferSerialiser;

        private readonly IVerifyObjectSerialiser verifySerialiser;

        private readonly IErrorResponseSerialiser errorResponseSerialiser;

        private readonly ILogger logger;

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

		/// <inheritdoc />
		public Task HandleBatchAction(GitHost host, string repositoryName, ObjectId objectId, BatchObjectAction action)
        {
            switch (action.Mode)
            {
                case BatchActionMode.Download:
                    return this.DownloadFile(host, repositoryName, objectId, action);
                case BatchActionMode.Upload:
                    return this.UploadFile(host, repositoryName, objectId, action);
                case BatchActionMode.Verify:
                    return this.Verify(host, repositoryName, objectId, action);
                default:
                    throw new LfsException($"Invalid Batch Action {action}");
                    
            }
        }

		/// <inheritdoc />
		public async Task<BatchTransfer> RequestBatch(GitHost host, string repositoryName, BatchRequest request)
        {
            using (HttpClient client = new HttpClient())
            {
                client.DefaultRequestHeaders.Accept.Clear();
                client.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("application/vnd.git-lfs+json"));
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

                    BatchTransfer transfer = this.transferSerialiser.TransferFromString(await result.Content.ReadAsStringAsync());

                    return transfer;
                }
            }
        }

		private static void SetClientHeaders(BatchObjectAction action, HttpClient downloadHttpClient)
		{
			if (action.Headers != null)
			{
				foreach (BatchHeader header in action.Headers)
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

        private async Task Verify(GitHost host, string repositoryName, ObjectId objectId, BatchObjectAction action)
        {
            using (var httpClient = new HttpClient())
            {
                SetClientHeaders(action, httpClient);

                using (var content = new StringContent(
                    this.verifySerialiser.ToString(objectId),
                    null,
                    "application/vnd.git-lfs+json"))
                {
					logger.LogInformation($"Verify from {action.HRef} with repository name {repositoryName}, request:{objectId.Hash.Substring(0, 10)}/{objectId.Size}");
					var result = await httpClient.PostAsync(action.HRef, content);

					if (result.IsSuccessStatusCode == false)
					{
						await this.HandleError(result);
					}
                }
            }
        }

		private async Task DownloadFile(GitHost host, string repositoryName, ObjectId objectId, BatchObjectAction action)
        {
            using (var httpClient = new HttpClient())
            {
                SetClientHeaders(action, httpClient);

				logger.LogInformation($"Download from {action.HRef} with repository name {repositoryName}, request:{objectId.Hash.Substring(0, 10)}/{objectId.Size}");
                var result = await httpClient.GetAsync(action.HRef, HttpCompletionOption.ResponseHeadersRead);

				if (result.IsSuccessStatusCode == false)
				{
					await this.HandleError(result);
				}

                await this.fileManager.SaveFile(repositoryName, objectId, FileLocation.Permenant, await result.Content.ReadAsStreamAsync());
            }
        }

        private async Task UploadFile(GitHost host, string repositoryName, ObjectId objectId, BatchObjectAction action)
        {
            using (var httpClient = new HttpClient())
            {
                SetClientHeaders(action, httpClient);

                Stream stream = this.fileManager.GetFile(repositoryName, objectId, FileLocation.Temporary);

                using (var content = new StreamContent(stream))
                {
                    content.Headers.ContentType = MediaTypeHeaderValue.Parse("application/octet-stream");
                    logger.LogInformation($"Uploading to {action.HRef} with repository name {repositoryName}, request:{objectId.Hash.Substring(0, 10)}/{objectId.Size}");
                    var result = await httpClient.PutAsync(action.HRef, content);

                    if (result.IsSuccessStatusCode == false)
                    {
						await this.HandleError(result);
					}
                }
            }
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
				errorResponse = new ErrorResponse() { Message = result.ReasonPhrase };
			}

			throw new ErrorResponseException(errorResponse, (int)result.StatusCode);
		}
	}
}
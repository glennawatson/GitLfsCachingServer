// <copyright file="FileCachingLfsClient.cs" company="Glenn Watson">
// Copyright (c) 2018 Glenn Watson. All rights reserved.
// See LICENSE file in the project root for full license information.
// </copyright>

namespace GitLfs.Client;

using System;
using System.IO;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Threading.Tasks;
using GitLfs.Core;
using GitLfs.Core.BatchRequest;
using GitLfs.Core.BatchResponse;
using GitLfs.Core.ErrorHandling;
using GitLfs.Core.File;
using GitLfs.Core.Verify;
using Microsoft.Extensions.Logging;

/// <summary>
/// A file caching based LFS client. This will handle caching to and from files.
/// </summary>
public class FileCachingLfsClient : ILfsClient
{
    private readonly IErrorResponseSerialiser _errorResponseSerialiser;

    private readonly IFileManager _fileManager;

    private readonly ILogger _logger;

    private readonly IBatchRequestSerialiser _requestSerialiser;

    private readonly IBatchTransferSerialiser _transferSerialiser;

    private readonly IVerifyObjectSerialiser _verifySerialiser;

    /// <summary>
    /// Initializes a new instance of the <see cref="FileCachingLfsClient"/> class.
    /// </summary>
    /// <param name="fileManager">A manager which will handle file uploads.</param>
    /// <param name="requestSerialiser">A serialiser that will serialise requests.</param>
    /// <param name="transferSerialiser">A serialiser that will serialise transfer requests.</param>
    /// <param name="verifySerialiser">A serialiser that will serialie verify requests.</param>
    /// <param name="errorResponseSerialiser">A serialiser that will serialise the errors.</param>
    /// <param name="logger">A logger where to send debug and error messages to.</param>
    public FileCachingLfsClient(
        IFileManager fileManager,
        IBatchRequestSerialiser requestSerialiser,
        IBatchTransferSerialiser transferSerialiser,
        IVerifyObjectSerialiser verifySerialiser,
        IErrorResponseSerialiser errorResponseSerialiser,
        ILogger<FileCachingLfsClient> logger)
    {
        _fileManager = fileManager;
        _requestSerialiser = requestSerialiser;
        _transferSerialiser = transferSerialiser;
        _verifySerialiser = verifySerialiser;
        _errorResponseSerialiser = errorResponseSerialiser;
        _logger = logger;
    }

    /// <inheritdoc />
    public async Task<Stream> DownloadFileAsync(
        GitHost host,
        string repositoryName,
        ObjectId objectId,
        BatchObjectAction action)
    {
        using (var httpClient = new HttpClient())
        {
            SetClientHeaders(action, httpClient);

            _logger.LogInformation(
                    $"Download from {action.HRef} with repository name {repositoryName}, request:{objectId.Hash.Substring(0, 10)}/{objectId.Size}");
            HttpResponseMessage result = await httpClient.GetAsync(action.HRef, HttpCompletionOption.ResponseHeadersRead).ConfigureAwait(false);

            if (!result.IsSuccessStatusCode)
            {
                await HandleErrorAsync(result).ConfigureAwait(false);
            }

            _logger.LogInformation(
                $"Now saving to a file {repositoryName}, request:{objectId.Hash.Substring(0, 10)}/{objectId.Size}\"");

            return _fileManager.SaveFile(
                out string fileName,
                repositoryName,
                objectId,
                FileLocation.Permanent,
                await result.Content.ReadAsStreamAsync().ConfigureAwait(false));
        }
    }

    /// <inheritdoc />
    public async Task<BatchTransfer> RequestBatchAsync(GitHost host, string repositoryName, BatchRequest request)
    {
        using (var client = new HttpClient())
        {
            client.DefaultRequestHeaders.Accept.Clear();
            client.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("application/vnd.git-lfs+json"));
            client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("token", host.Token);

            using (var content = new StringContent(
                _requestSerialiser.ToString(request),
                null,
                "application/vnd.git-lfs+json"))
            {
                HttpResponseMessage result = await client.PostAsync(GetLfsBatchUrl(host, repositoryName), content).ConfigureAwait(false);

                if (!result.IsSuccessStatusCode)
                {
                    await HandleErrorAsync(result).ConfigureAwait(false);
                }

                string returnContents = await result.Content.ReadAsStringAsync().ConfigureAwait(false);
                return _transferSerialiser.TransferFromString(returnContents);
            }
        }
    }

    /// <inheritdoc />
    public async Task UploadFileAsync(BatchObjectAction action, Stream stream)
    {
        using (var httpClient = new HttpClient())
        {
            SetClientHeaders(action, httpClient);

            var content = new StreamContent(stream, 2000);
            content.Headers.ContentType = MediaTypeHeaderValue.Parse("application/octet-stream");
            _logger.LogInformation($"Uploading to {action.HRef}");
            HttpResponseMessage result = await httpClient.PutAsync(action.HRef, content).ConfigureAwait(false);

            if (!result.IsSuccessStatusCode)
            {
                _logger.LogInformation($"Failed uploading to {action.HRef}");
                await HandleErrorAsync(result).ConfigureAwait(false);
            }
            else
            {
                _logger.LogInformation(
                    $"Success uploading to {action.HRef}");
            }
        }
    }

    /// <inheritdoc />
    public async Task VerifyAsync(GitHost host, string repositoryName, ObjectId objectId, BatchObjectAction action)
    {
        using (var httpClient = new HttpClient())
        {
            SetClientHeaders(action, httpClient);

            using (var content = new StringContent(
                _verifySerialiser.ToString(objectId),
                null,
                "application/vnd.git-lfs+json"))
            {
                _logger.LogInformation(
                    $"Verify from {action.HRef} with repository name {repositoryName}, request:{objectId.Hash.Substring(0, 10)}/{objectId.Size}");
                HttpResponseMessage result = await httpClient.PostAsync(action.HRef, content).ConfigureAwait(false);

                if (!result.IsSuccessStatusCode)
                {
                    await HandleErrorAsync(result).ConfigureAwait(false);
                }
            }
        }
    }

    private static Uri GetLfsBatchUrl(GitHost host, string repositoryName)
    {
        return new Uri($"{host.Href}/{repositoryName}/info/lfs/objects/batch");
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

    private async Task HandleErrorAsync(HttpResponseMessage result)
    {
        ErrorResponse errorResponse;
        try
        {
            errorResponse = _errorResponseSerialiser.FromString(await result.Content.ReadAsStringAsync().ConfigureAwait(false));
        }
        catch (ParseException)
        {
            errorResponse = new ErrorResponse { Message = result.ReasonPhrase };
        }

        _logger.LogWarning($"Operation failed. {errorResponse}");

        throw new ErrorResponseException(errorResponse, (int)result.StatusCode);
    }
}

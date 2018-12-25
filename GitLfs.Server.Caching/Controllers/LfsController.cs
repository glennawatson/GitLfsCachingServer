// <copyright file="LfsController.cs" company="Glenn Watson">
// Copyright (c) 2018 Glenn Watson. All rights reserved.
// See LICENSE file in the project root for full license information.
// </copyright>

namespace GitLfs.Server.Caching.Controllers
{
    using System;
    using System.Collections.Generic;
    using System.Diagnostics.CodeAnalysis;
    using System.IO;
    using System.Threading.Tasks;
    using GitLfs.Client;
    using GitLfs.Core;
    using GitLfs.Core.BatchRequest;
    using GitLfs.Core.BatchResponse;
    using GitLfs.Core.ErrorHandling;
    using GitLfs.Core.File;
    using GitLfs.Server.Caching.Data;
    using Microsoft.AspNetCore.Mvc;
    using Microsoft.Extensions.Logging;

    /// <summary>
    /// Controller for handling LFS data.
    /// </summary>
    [Produces("application/vnd.git-lfs+json")]
    public class LfsController : Controller
    {
        private const int DefaultErrorCode = 404;

        private readonly ApplicationDbContext context;

        private readonly IFileManager fileManager;

        private readonly ILfsClient lfsClient;

        private readonly IBatchTransferSerialiser transferSerialiser;

        private readonly ILogger logger;

        /// <summary>
        /// Initializes a new instance of the <see cref="LfsController"/> class.
        /// </summary>
        /// <param name="context">The main database context.</param>
        /// <param name="fileManager">A instance to the file manager.</param>
        /// <param name="lfsClient">The main LFS client.</param>
        /// <param name="transferSerialiser">A serialiser that will serialise the batch transfer requests.</param>
        /// <param name="logger">The main logger for this part of the application.</param>
        public LfsController(
            ApplicationDbContext context,
            IFileManager fileManager,
            ILfsClient lfsClient,
            IBatchTransferSerialiser transferSerialiser,
            ILogger<LfsController> logger)
        {
            this.context = context;
            this.fileManager = fileManager;
            this.lfsClient = lfsClient;
            this.transferSerialiser = transferSerialiser;
            this.logger = logger;
        }

        /// <summary>
        /// Downloads a file from the LFS store.
        /// </summary>
        /// <param name="hostId">The internal identification of the host.</param>
        /// <param name="repositoryName">The name of the repository where to download the file from.</param>
        /// <param name="objectId">The ID of the object to download.</param>
        /// <param name="size">The size of the object to download.</param>
        /// <returns>The result of the action.</returns>
        [HttpGet("/api/{hostId}/{repositoryName}/info/lfs/{objectId}/{size}")]
        public async Task<IActionResult> DownloadFile(int hostId, string repositoryName, string objectId, long size)
        {
            var fileObjectId = new ObjectId(objectId, size);

            try
            {
                GitHost host = await this.context.GitHost.FindAsync(hostId).ConfigureAwait(false);

                if (host == null)
                {
                    return this.NotFound(new ErrorResponse { Message = "Not a valid host id." });
                }

                Stream stream = this.fileManager.GetFileStream(repositoryName, fileObjectId, FileLocation.Permanent);

                if (stream != null)
                {
                    this.logger.LogInformation($"Found local cache file: {fileObjectId}");
                    return this.File(stream, "application/octet-stream");
                }

                var actionStream = await this.fileManager.GetFileContentsAsync(repositoryName, fileObjectId, FileLocation.Metadata, "download").ConfigureAwait(false);

                if (actionStream == null)
                {
                    this.logger.LogWarning($"No valid batch request for {fileObjectId}");
                    return this.NotFound(new ErrorResponse { Message = $"No valid batch request for {fileObjectId}" });
                }

                BatchObjectAction action = this.transferSerialiser.ObjectActionFromString(actionStream);

                if (action == null)
                {
                    this.logger.LogWarning($"Unable to find file {objectId}");
                    return this.NotFound(new ErrorResponse { Message = $"Unable to find file {objectId}" });
                }

                if (action.Mode != BatchActionMode.Download)
                {
                    this.logger.LogWarning($"No download action associated with request: {objectId}");
                    return this.StatusCode(
                        422,
                        new ErrorResponse { Message = $"No download action associated with request: {objectId}" });
                }

                this.fileManager.DeleteFile(repositoryName, fileObjectId, FileLocation.Metadata, "download");

                return this.File(
                    await this.lfsClient.DownloadFileAsync(
                                    host,
                                    repositoryName,
                                    fileObjectId,
                                    action).ConfigureAwait(false),
                    "application/octet-stream");
            }
            catch (ErrorResponseException ex)
            {
                this.logger.LogWarning(null, ex, $"Received a error response with message {ex.Message} for {fileObjectId}");
                return this.StatusCode(ex.StatusCode ?? DefaultErrorCode, ex.ErrorResponse);
            }
            catch (StatusCodeException ex)
            {
                this.logger.LogWarning(null, ex, $"Received a status code error with message {ex.Message}");
                return this.StatusCode(ex.StatusCode ?? DefaultErrorCode, new ErrorResponse { Message = ex.Message });
            }
        }

        /// <summary>
        /// Handles a request for batching a transfer.
        /// </summary>
        /// <param name="hostId">The internal identification of the host.</param>
        /// <param name="repositoryName">The name of the repository where to download the file from.</param>
        /// <param name="request">The batch request being asked for.</param>
        /// <returns>The result of the action.</returns>
        [SuppressMessage("Globalization", "CA1308:Normalize strings to uppercase", Justification = "Needed by LFS standard.")]
        [HttpPost("/api/{hostId}/{repositoryName}/info/lfs/objects/batch")]
        public async Task<IActionResult> HandleBatchRequest(
            int hostId,
            string repositoryName,
            [FromBody] BatchRequest request)
        {
            GitHost host = await this.context.GitHost.FindAsync(hostId).ConfigureAwait(false);

            if (host == null)
            {
                return this.NotFound(new ErrorResponse { Message = "Not a valid host id." });
            }

            var serverBatchRequest = new BatchRequest
            {
                Objects = new List<ObjectId>(),
                Operation = request.Operation,
                Transfers = request.Transfers
            };

            foreach (ObjectId objectId in request.Objects)
            {
                if (request.Operation == BatchRequestMode.Download
                    && this.fileManager.IsFileStored(repositoryName, objectId, FileLocation.Permanent))
                {
                    continue;
                }

                serverBatchRequest.Objects.Add(objectId);
            }

            try
            {
                IDictionary<ObjectId, IBatchObject> batchObjects = new Dictionary<ObjectId, IBatchObject>();
                if (serverBatchRequest.Objects.Count > 0)
                {
                    BatchTransfer serverResults =
                        await this.lfsClient.RequestBatchAsync(host, repositoryName, serverBatchRequest).ConfigureAwait(false);
                    foreach (IBatchObject serverResult in serverResults.Objects)
                    {
                        batchObjects.Add(serverResult.Id, serverResult);

                        if (serverResult is BatchObject batchObject)
                        {
                            foreach (BatchObjectAction action in batchObject.Actions)
                            {
                                await this.fileManager.SaveFileAsync(
                                    repositoryName,
                                    serverResult.Id,
                                    FileLocation.Metadata,
                                    this.transferSerialiser.ToString(action),
                                    action.Mode.ToString().ToLowerInvariant()).ConfigureAwait(false);
                            }
                        }
                    }
                }

                var returnResult = new BatchTransfer();
                returnResult.Mode = TransferMode.Basic;
                returnResult.Objects = new List<IBatchObject>();

                foreach (ObjectId pendingObjectId in request.Objects)
                {
                    batchObjects.TryGetValue(pendingObjectId, out IBatchObject batchObjectBase);
                    var batchObject = batchObjectBase as BatchObject;

                    if (batchObjectBase is BatchObjectError errorResult)
                    {
                        returnResult.Objects.Add(errorResult);
                    }
                    else
                    {
                        switch (request.Operation)
                        {
                            case BatchRequestMode.Upload:
                                returnResult.Objects.Add(batchObject);
                                break;
                            case BatchRequestMode.Download:
                                {
                                    var returnBatchObject = new BatchObject { Id = pendingObjectId, Actions = new List<BatchObjectAction>() };
                                    var action = new BatchObjectAction
                                    {
                                        Mode = BatchActionMode.Download,
                                        HRef =
                                            $"{this.Request.Scheme}://{this.Request.Host}/api/{hostId}/{repositoryName}/info/lfs/{pendingObjectId.Hash}/{pendingObjectId.Size}"
                                    };

                                    returnBatchObject.Actions.Add(action);
                                    returnResult.Objects.Add(returnBatchObject);
                                    break;
                                }
                        }
                    }
                }

                return this.Ok(returnResult);
            }
            catch (ErrorResponseException ex)
            {
                this.logger.LogWarning(null, ex, $"Received a error response with message {ex.Message}");
                return this.StatusCode(ex.StatusCode ?? DefaultErrorCode, ex.ErrorResponse);
            }
            catch (StatusCodeException ex)
            {
                this.logger.LogWarning(null, ex, $"Received a status code error with message {ex.Message}");
                return this.StatusCode(ex.StatusCode ?? DefaultErrorCode, new ErrorResponse { Message = ex.Message });
            }
        }

        /// <summary>
        /// Handles a upload request to the LFS store.
        /// </summary>
        /// <param name="hostId">The internal identification of the host.</param>
        /// <param name="repositoryName">The name of the repository where to upload the file to.</param>
        /// <param name="objectId">The ID of the object to upload.</param>
        /// <param name="size">The size of the object to upload.</param>
        /// <returns>The result of the action.</returns>
        [HttpPut("/api/{hostId}/{repositoryName}/info/lfs/{objectId}/{size}")]
        public async Task<IActionResult> UploadFile(int hostId, string repositoryName, string objectId, long size)
        {
            var fileObjectId = new ObjectId(objectId, size);

            try
            {
                GitHost host = await this.context.GitHost.FindAsync(hostId).ConfigureAwait(false);

                if (host == null)
                {
                    this.logger.LogWarning("Not a valid host id");
                    return this.NotFound(new ErrorResponse { Message = "Not a valid host id" });
                }

                this.logger.LogInformation($"Saving the file to disk for {fileObjectId}");

                this.Response.Headers.Remove("transfer-encoding");

                if (!this.fileManager.IsFileStored(
                        repositoryName,
                        fileObjectId,
                        FileLocation.Metadata,
                        false,
                        "upload"))
                {
                    this.fileManager.DeleteFile(repositoryName, fileObjectId, FileLocation.Metadata, "upload");
                    return this.Ok();
                }

                BatchObjectAction action = this.transferSerialiser.ObjectActionFromString(await this.fileManager.GetFileContentsAsync(repositoryName, fileObjectId, FileLocation.Metadata, "upload").ConfigureAwait(false));

                this.logger.LogInformation($"Starting file upload for {fileObjectId}");
                await this.lfsClient.UploadFileAsync(action, this.Request.Body).ConfigureAwait(false);
                this.logger.LogInformation($"Finished file upload for {fileObjectId}");

                this.fileManager.DeleteFile(repositoryName, fileObjectId, FileLocation.Metadata, "upload");
                return this.Ok();
            }
            catch (ErrorResponseException ex)
            {
                this.logger.LogWarning(null, ex, $"Received a error response with message {ex.Message} for {fileObjectId}");
                this.fileManager.DeleteFile(repositoryName, fileObjectId, FileLocation.Temporary);
                return this.StatusCode(ex.StatusCode ?? DefaultErrorCode, ex.ErrorResponse);
            }
            catch (StatusCodeException ex)
            {
                this.logger.LogWarning(null, ex, $"Received a status code error with message {ex.Message} for {fileObjectId}");
                this.fileManager.DeleteFile(repositoryName, fileObjectId, FileLocation.Temporary);
                return this.StatusCode(ex.StatusCode ?? DefaultErrorCode, new ErrorResponse { Message = ex.Message });
            }
        }

        /// <summary>
        /// Handles a verify request to the LFS service.
        /// </summary>
        /// <param name="hostId">The internal identification of the host.</param>
        /// <param name="repositoryName">The name of the repository where to verify the file from.</param>
        /// <param name="objectId">The ID of the object to verify.</param>
        /// <param name="size">The size of the object to verify.</param>
        /// <returns>The result of the action.</returns>
        [HttpPost("/api/{hostId}/{repositoryName}/info/lfs/verify/{objectId}/{size}")]
        public async Task<IActionResult> Verify(int hostId, string repositoryName, string objectId, long size)
        {
            var fileObjectId = new ObjectId(objectId, size);

            try
            {
                GitHost host = await this.context.GitHost.FindAsync(hostId).ConfigureAwait(false);

                if (host == null)
                {
                    return this.NotFound(new ErrorResponse { Message = "Not a valid host id." });
                }

                this.fileManager.MoveFile(repositoryName, fileObjectId, FileLocation.Temporary, FileLocation.Permanent);

                if (this.fileManager.IsFileStored(repositoryName, fileObjectId, FileLocation.Metadata, false, "verify"))
                {
                    this.logger.LogInformation($"Starting verify for {fileObjectId}");
                    BatchObjectAction action = this.transferSerialiser.ObjectActionFromString(await this.fileManager.GetFileContentsAsync(repositoryName, fileObjectId, FileLocation.Metadata, "verify").ConfigureAwait(false));
                    await this.lfsClient.VerifyAsync(host, repositoryName, fileObjectId, action).ConfigureAwait(false);
                    this.logger.LogInformation($"Ending verify for {fileObjectId}");
                }

                this.fileManager.DeleteFile(repositoryName, fileObjectId, FileLocation.Metadata, "verify");

                return this.Ok();
            }
            catch (ErrorResponseException ex)
            {
                this.logger.LogWarning(null, ex, $"Received a error response with message {ex.Message} for {fileObjectId}");
                return this.StatusCode(ex.StatusCode ?? DefaultErrorCode, ex.ErrorResponse);
            }
            catch (StatusCodeException ex)
            {
                this.logger.LogWarning(null, ex, $"Received a status code error with message {ex.Message} for {fileObjectId}");
                return this.StatusCode(ex.StatusCode ?? DefaultErrorCode, new ErrorResponse { Message = ex.Message });
            }
        }
    }
}
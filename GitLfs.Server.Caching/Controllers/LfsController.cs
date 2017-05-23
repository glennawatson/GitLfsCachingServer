// <copyright file="LfsController.cs" company="Glenn Watson">
//    Copyright (C) 2017. Glenn Watson
// </copyright>

namespace GitLfs.Server.Caching.Controllers
{
    using System;
    using System.Collections.Generic;
    using System.IO;
    using System.Threading.Tasks;

    using GitLfs.Client;
    using GitLfs.Core;
    using GitLfs.Core.BatchRequest;
    using GitLfs.Core.BatchResponse;
    using GitLfs.Core.Error;
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
        private readonly ApplicationDbContext context;

        private readonly IFileManager fileManager;

        private readonly ILfsClient lfsClient;

        private readonly IBatchTransferSerialiser transferSerialiser;

        private readonly ILogger logger;

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

        [HttpGet("/api/{hostId}/{repositoryName}/info/lfs/{objectId}/{size}")]
        public async Task<IActionResult> DownloadFile(int hostId, string repositoryName, string objectId, long size)
        {
            var fileObjectId = new ObjectId(objectId, size);

            try
            {
                GitHost host = await this.context.GitHost.FindAsync(hostId);

                if (host == null)
                {
                    return this.NotFound(new ErrorResponse { Message = "Not a valid host id." });
                }

                Stream stream = this.fileManager.GetFileStream(repositoryName, fileObjectId, FileLocation.Permenant);

                if (stream != null)
                {
                    return this.File(stream, "application/octet-stream");
                }

                var actionStream = await this.fileManager.GetFileContentsAsync(repositoryName, fileObjectId, FileLocation.Metadata, "download");

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
                    await this.lfsClient.DownloadFile(
                                    host,
                                    repositoryName,
                                    fileObjectId,
                                    action),
                    "application/octet-stream");
            }
            catch (ErrorResponseException ex)
            {
                this.logger.LogWarning(null, ex, $"Received a error response with message {ex.Message} for {fileObjectId}");
                return this.StatusCode(ex.StatusCode.Value, ex.ErrorResponse);
            }
            catch (StatusCodeException ex)
            {
                this.logger.LogWarning(null, ex, $"Received a status code error with message {ex.Message}");
                return this.StatusCode(ex.StatusCode.Value, new ErrorResponse { Message = ex.Message });
            }
        }

        [HttpPost("/api/{hostId}/{repositoryName}/info/lfs/objects/batch")]
        public async Task<IActionResult> HandleBatchRequest(
            int hostId,
            string repositoryName,
            [FromBody] BatchRequest request)
        {
            GitHost host = await this.context.GitHost.FindAsync(hostId);

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
                    && this.fileManager.IsFileStored(repositoryName, objectId, FileLocation.Permenant))
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
                        await this.lfsClient.RequestBatch(host, repositoryName, serverBatchRequest);
                    foreach (IBatchObject serverResult in serverResults.Objects)
                    {
                        batchObjects.Add(serverResult.Id, serverResult);

                        BatchObject batchObject = serverResult as BatchObject;

                        if (batchObject != null)
                        {
                            foreach (BatchObjectAction action in batchObject.Actions)
                            {
                                await this.fileManager.SaveFileAsync(
                                    repositoryName,
                                    serverResult.Id,
                                    FileLocation.Metadata,
                                    this.transferSerialiser.ToString(action),
                                    action.Mode.ToString().ToLowerInvariant());
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
                    var errorResult = batchObjectBase as BatchObjectError;
                    var batchObject = batchObjectBase as BatchObject;

                    if (errorResult != null)
                    {
                        returnResult.Objects.Add(errorResult);
                    }
                    else if (request.Operation == BatchRequestMode.Upload)
                    {
                        returnResult.Objects.Add(batchObject);
                    }
                    else if (request.Operation == BatchRequestMode.Download)
                    {
                        var returnBatchObject = new BatchObject { Id = pendingObjectId, Actions = new List<BatchObjectAction>() };
                        var action = new BatchObjectAction();
                        action.Mode = BatchActionMode.Download;
                        action.HRef =
                            $"{this.Request.Scheme}://{this.Request.Host}/api/{hostId}/{repositoryName}/info/lfs/{pendingObjectId.Hash}/{pendingObjectId.Size}";

                        returnBatchObject.Actions.Add(action);
                        returnResult.Objects.Add(returnBatchObject);
                    }
                }

                return this.Ok(returnResult);
            }
            catch (ErrorResponseException ex)
            {
                this.logger.LogWarning(null, ex, $"Received a error response with message {ex.Message}");
                return this.StatusCode(ex.StatusCode.Value, ex.ErrorResponse);
            }
            catch (StatusCodeException ex)
            {
                this.logger.LogWarning(null, ex, $"Received a status code error with message {ex.Message}");
                return this.StatusCode(ex.StatusCode.Value, new ErrorResponse { Message = ex.Message });
            }
        }

        [HttpPut("/api/{hostId}/{repositoryName}/info/lfs/{objectId}/{size}")]
        public async Task<IActionResult> UploadFile(int hostId, string repositoryName, string objectId, long size)
        {
            var fileObjectId = new ObjectId(objectId, size);

            try
            {
                GitHost host = await this.context.GitHost.FindAsync(hostId);

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

                BatchObjectAction action = this.transferSerialiser.ObjectActionFromString(await this.fileManager.GetFileContentsAsync(repositoryName, fileObjectId, FileLocation.Metadata, "upload"));


                this.logger.LogInformation($"Starting file upload for {fileObjectId}");
                await this.lfsClient.UploadFile(action, this.Request.Body);
                this.logger.LogInformation($"Finished file upload for {fileObjectId}");

                this.fileManager.DeleteFile(repositoryName, fileObjectId, FileLocation.Metadata, "upload");
                return this.Ok();
            }
            catch (ErrorResponseException ex)
            {
                this.logger.LogWarning(null, ex, $"Received a error response with message {ex.Message} for {fileObjectId}");
                this.fileManager.DeleteFile(repositoryName, fileObjectId, FileLocation.Temporary);
                return this.StatusCode(ex.StatusCode.Value, ex.ErrorResponse);
            }
            catch (StatusCodeException ex)
            {
                this.logger.LogWarning(null, ex, $"Received a status code error with message {ex.Message} for {fileObjectId}");
                this.fileManager.DeleteFile(repositoryName, fileObjectId, FileLocation.Temporary);
                return this.StatusCode(ex.StatusCode.Value, new ErrorResponse { Message = ex.Message });
            }
        }

        [HttpPost("/api/{hostId}/{repositoryName}/info/lfs/verify/{objectId}/{size}")]
        public async Task<IActionResult> Verify(int hostId, string repositoryName, string objectId, long size)
        {
            var fileObjectId = new ObjectId(objectId, size);

            try
            {
                GitHost host = await this.context.GitHost.FindAsync(hostId);

                if (host == null)
                {
                    return this.NotFound(new ErrorResponse { Message = "Not a valid host id." });
                }

                this.fileManager.MoveFile(repositoryName, fileObjectId, FileLocation.Temporary, FileLocation.Permenant);

                if (this.fileManager.IsFileStored(repositoryName, fileObjectId, FileLocation.Metadata, false, "verify"))
                {
                    this.logger.LogInformation($"Starting verify for {fileObjectId}");
                    BatchObjectAction action = this.transferSerialiser.ObjectActionFromString(await this.fileManager.GetFileContentsAsync(repositoryName, fileObjectId, FileLocation.Metadata, "verify"));
                    await this.lfsClient.Verify(host, repositoryName, fileObjectId, action);
                    this.logger.LogInformation($"Ending verify for {fileObjectId}");
                }


                this.fileManager.DeleteFile(repositoryName, fileObjectId, FileLocation.Metadata, "verify");

                return this.Ok();
            }
            catch (ErrorResponseException ex)
            {
                this.logger.LogWarning(null, ex, $"Received a error response with message {ex.Message} for {fileObjectId}");
                return this.StatusCode(ex.StatusCode.Value, ex.ErrorResponse);
            }
            catch (StatusCodeException ex)
            {
                this.logger.LogWarning(null, ex, $"Received a status code error with message {ex.Message} for {fileObjectId}");
                return this.StatusCode(ex.StatusCode.Value, new ErrorResponse { Message = ex.Message });
            }
        }
    }
}
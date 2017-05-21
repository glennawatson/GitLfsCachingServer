// --------------------------------------------------------------------------------------------------------------------
// <copyright file="LfsController.cs" company="Glenn Watson">
//     Copyright (C) 2017. Glenn Watson
// </copyright>
// --------------------------------------------------------------------------------------------------------------------

namespace GitLfs.Server.Caching.Controllers
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.IO;
    using System.Threading.Tasks;

    using GitLfs.Client;
    using GitLfs.Core;
    using GitLfs.Core.BatchRequest;
    using GitLfs.Core.BatchResponse;
    using GitLfs.Core.Error;
    using GitLfs.Core.File;
    using GitLfs.Server.Caching.Data;
    using GitLfs.Server.Caching.Models;

    using Microsoft.AspNetCore.Mvc;
    using Microsoft.EntityFrameworkCore;

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

        public LfsController(ApplicationDbContext context, IFileManager fileManager, ILfsClient lfsClient, IBatchTransferSerialiser transferSerialiser)
        {
            this.context = context;
            this.fileManager = fileManager;
            this.lfsClient = lfsClient;
            this.transferSerialiser = transferSerialiser;
        }

        [HttpGet("/api/{hostId}/{repositoryName}/info/lfs/{objectId}/{size}")]
        public async Task<IActionResult> DownloadFile(int hostId, string repositoryName, string objectId, long size)
        {
            try
            {
                GitHost host = await this.context.GitHost.FindAsync(hostId);

                if (host == null)
                {
                    return this.NotFound(new ErrorResponse { Message = "Not a valid host id." });
                }

                var fileObjectId = new ObjectId(objectId, size);

                if (this.fileManager.IsFileStored(repositoryName, fileObjectId, FileLocation.Metadata))
                {
                    using (var streamer = new StreamReader(this.fileManager.GetFile(repositoryName, fileObjectId, FileLocation.Metadata)))
                    {
                        var objectDetails = this.transferSerialiser.ObjectFromString(await streamer.ReadToEndAsync());
						
                        if (objectDetails != null)
						{
							if (objectDetails.Actions.Count != 1 ||
								objectDetails.Actions[0].Mode != BatchActionMode.Download)
							{
								return this.StatusCode(422, new ErrorResponse { Message = "No download action associated with request" });
							}

							await this.lfsClient.HandleBatchAction(host, repositoryName, fileObjectId, objectDetails.Actions[0]);
						}
					}
                }

                Stream stream = this.fileManager.GetFile(repositoryName, fileObjectId, FileLocation.Permenant);
              
                this.fileManager.DeleteFile(repositoryName, fileObjectId, FileLocation.Metadata);

                return this.File(stream, "application/octet-stream");
            }
            catch (ErrorResponseException ex)
            {
                return StatusCode(ex.StatusCode.Value, ex.ErrorResponse);
            }
            catch (StatusCodeException ex)
            {
                return this.StatusCode(ex.StatusCode.Value, new ErrorResponse { Message = ex.Message });
            }
        }

        [HttpPut("/api/{hostId}/{repositoryName}/info/lfs/{objectId}/{size}")]
        public async Task<IActionResult> UploadFile(int hostId, string repositoryName, string objectId, long size)
        {
            try
            {
                GitHost host = await this.context.GitHost.FindAsync(hostId);

                if (host == null)
                {
                    return this.NotFound(new ErrorResponse { Message = "Not a valid host id." });
                }

				var fileObjectId = new ObjectId(objectId, size);

                BatchObject objectDetails = null;

                if (this.fileManager.IsFileStored(repositoryName, fileObjectId, FileLocation.Metadata))
                {
                    using (var streamer = new StreamReader(this.fileManager.GetFile(repositoryName, fileObjectId, FileLocation.Metadata)))
                    {
                        objectDetails = this.transferSerialiser.ObjectFromString(await streamer.ReadToEndAsync());
                    }
                }

                if (objectDetails == null ||
                    objectDetails.Actions.Count == 0 ||
                    objectDetails.Actions.Count > 2 ||
                    objectDetails.Actions[0].Mode != BatchActionMode.Upload)
                {
                    return this.StatusCode(422, new ErrorResponse { Message = "No upload action associated with request" });
                }


                Stream body = this.HttpContext.Request.Body;

                await this.fileManager.SaveFile(repositoryName, fileObjectId, FileLocation.Temporary, body);

                await this.lfsClient.HandleBatchAction(host, repositoryName, fileObjectId, objectDetails.Actions[0]);

                return this.Ok();
            }
            catch (ErrorResponseException ex)
            {
                return StatusCode(ex.StatusCode.Value, ex.ErrorResponse);
            }
            catch (StatusCodeException ex)
            {
                return this.StatusCode(ex.StatusCode.Value, new ErrorResponse { Message = ex.Message });
            }
        }

        [HttpPost("/api/{hostId}/{repositoryName}/info/lfs/verify/{objectId}/{size}")]
        public async Task<IActionResult> Verify(int hostId, string repositoryName, string objectId, long size)
        {
            try
            {
                GitHost host = await this.context.GitHost.FindAsync(hostId);

                if (host == null)
                {
                    return this.NotFound(new ErrorResponse { Message = "Not a valid host id." });
                }

				var fileObjectId = new ObjectId(objectId, size);

				BatchObject objectDetails = null;

				if (this.fileManager.IsFileStored(repositoryName, fileObjectId, FileLocation.Metadata))
				{
					using (var streamer = new StreamReader(this.fileManager.GetFile(repositoryName, fileObjectId, FileLocation.Metadata)))
					{
						objectDetails = this.transferSerialiser.ObjectFromString(await streamer.ReadToEndAsync());
					}
				}
  
                if (objectDetails == null ||
                    objectDetails.Actions.Count != 2 ||
                    objectDetails.Actions[0].Mode != BatchActionMode.Upload ||
                    objectDetails.Actions[1].Mode != BatchActionMode.Verify)
                {
                    return this.StatusCode(422, new ErrorResponse { Message = "Invalid Batch Request" });
                }

				await this.lfsClient.HandleBatchAction(host, repositoryName, fileObjectId, objectDetails.Actions[0]);

                await this.fileManager.MoveFile(repositoryName, objectDetails.Id, FileLocation.Temporary, FileLocation.Permenant);

                return this.Ok();
            }
            catch (ErrorResponseException ex)
            {
                return StatusCode(ex.StatusCode.Value, ex.ErrorResponse);
            }
            catch (StatusCodeException ex)
            {
                return this.StatusCode(ex.StatusCode.Value, new ErrorResponse { Message = ex.Message });
            }
        }

        [HttpPost("/api/{hostId}/{repositoryName}/info/lfs/objects/batch")]
        public async Task<IActionResult> HandleBatchRequest(
            int hostId,
            string repositoryName,
            [FromBody]BatchRequest request)
        {
            GitHost host = await this.context.GitHost.FindAsync(hostId);

            if (host == null)
            {
                return this.NotFound(new ErrorResponse { Message = "Not a valid host id." });
            }

            BatchRequest serverBatchRequest = new BatchRequest();
            serverBatchRequest.Objects = new List<ObjectId>();
            serverBatchRequest.Operation = request.Operation;
            serverBatchRequest.Transfers = request.Transfers;

            foreach (var objectId in request.Objects)
            {
                if (request.Operation == BatchRequestMode.Download && this.fileManager.IsFileStored(repositoryName, objectId, FileLocation.Permenant))
                {
                    continue;
                }

                serverBatchRequest.Objects.Add(objectId);
            }

            try
            {
                if (serverBatchRequest.Objects.Count > 0)
                {
                    var serverResults = await this.lfsClient.RequestBatch(host, repositoryName, serverBatchRequest);
                    foreach (var serverResult in serverResults.Objects.OfType<BatchObject>())
                    {
                        await this.fileManager.SaveFile(repositoryName, serverResult.Id, FileLocation.Metadata, this.transferSerialiser.ToString(serverResult as BatchObject));
                    }
                }

                BatchTransfer returnResult = new BatchTransfer();
                returnResult.Mode = TransferMode.Basic;
                returnResult.Objects = new List<IBatchObject>();

                foreach (var pendingRequest in request.Objects)
                {
                    BatchObjectError errorResult = null; //serverResults.Objects.SingleOrDefault(x => x.Id == pendingRequest.ObjectId) as BatchObjectError;

                    if (errorResult != null)
                    {
                        returnResult.Objects.Add(errorResult);
                    }
                    else if (request.Operation == BatchRequestMode.Download)
                    {
                        var action = new BatchObjectAction();
                        action.Mode = BatchActionMode.Download;
                        action.HRef = $"{this.Request.Scheme}://{this.Request.Host}/api/{hostId}/{repositoryName}/info/lfs/{pendingRequest.Hash}/{pendingRequest.Size}";

                        returnResult.Objects.Add(new BatchObject() { Id = pendingRequest, Actions = new List<BatchObjectAction> { action } });
                    }
                    else if (request.Operation == BatchRequestMode.Upload)
                    {
                        var uploadLocation = new Uri($"{this.Request.Scheme}://{this.Request.Host}/api/{hostId}/{repositoryName}/info/lfs/{pendingRequest.Hash}/{pendingRequest.Size}");

                        var uploadAction = new BatchObjectAction();
                        uploadAction.Mode = BatchActionMode.Upload;
                        uploadAction.HRef = $"{this.Request.Scheme}://{this.Request.Host}/api/{hostId}/{repositoryName}/info/lfs/{pendingRequest.Hash}/{pendingRequest.Size}";

                        var verifyAction = new BatchObjectAction();
                        verifyAction.Mode = BatchActionMode.Verify;
                        verifyAction.HRef = $"{this.Request.Scheme}://{this.Request.Host}/api/{hostId}/{repositoryName}/info/lfs/verify/{pendingRequest.Hash}/{pendingRequest.Size}";

                        returnResult.Objects.Add(new BatchObject() { Id = pendingRequest, Actions = new List<BatchObjectAction> { uploadAction, verifyAction } });
                    }
                }

                return this.Ok(returnResult);

            }
            catch (ErrorResponseException ex)
            {
                return this.StatusCode(ex.StatusCode.Value, ex.ErrorResponse);
            }
            catch (StatusCodeException ex)
            {
                return this.StatusCode(ex.StatusCode.Value, new ErrorResponse() { Message = ex.Message });
            }
        }
    }
}
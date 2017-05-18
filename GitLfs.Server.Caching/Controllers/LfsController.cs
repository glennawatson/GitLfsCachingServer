// --------------------------------------------------------------------------------------------------------------------
// <copyright file="LfsController.cs" company="Glenn Watson">
//     Copyright (C) 2017. Glenn Watson
// </copyright>
// --------------------------------------------------------------------------------------------------------------------

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
    using GitLfs.Core.Managers;
    using GitLfs.Server.Caching.Data;
    using GitLfs.Server.Caching.Models;

    using Microsoft.AspNetCore.Mvc;
    using Microsoft.EntityFrameworkCore;

    /// <summary>
    /// Controller for handling LFS data.
    /// </summary>
    [Produces("application/json")]
    [Route("api/Lfs")]
    public class LfsController : Controller
    {
        private readonly ApplicationDbContext context;

        private readonly IFileManager fileManager;

        private readonly ILfsClient lfsClient;

        private readonly IRequestSerialiser requestSerialiser;

        private readonly ITransferSerialiser transferSerialiser;

        public LfsController(
            ApplicationDbContext context,
            IFileManager fileManager,
            IRequestSerialiser requestSerialiser,
            ITransferSerialiser transferSerialiser,
            ILfsClient lfsClient)
        {
            this.context = context;
            this.fileManager = fileManager;
            this.lfsClient = lfsClient;
            this.requestSerialiser = requestSerialiser;
            this.transferSerialiser = transferSerialiser;
        }

        [HttpGet("/api/{hostId}/{repositoryName}/info/lfs/{objectId}")]
        public async Task<IActionResult> DownloadFile(int hostId, string repositoryName, string objectId)
        {
            try
            {
                GitHost host = await this.context.GitHost.FindAsync(hostId);

                if (host == null)
                {
                    return this.NotFound(new ErrorResponse { Message = "Not a valid host id." });
                }

                Stream stream = this.fileManager.GetFileForObjectId(repositoryName, objectId);

                if (stream == null)
                {
                    stream = await this.lfsClient.DownloadFile(
                                 host,
                                 repositoryName,
                                 new RequestObject { ObjectId = objectId, Size = 0 });
                }

                return this.File(stream, "application/octet-stream");
            }
            catch (ClientDownloadException ex)
            {
                return this.StatusCode(ex.StatusCode, new ErrorResponse { Message = ex.Message });
            }
        }

        [HttpPost("/api/{hostId}/{repositoryName}/info/lfs/objects/batch")]
        public async Task<IActionResult> HandleBatchRequest(
            int hostId,
            string repositoryName,
            [FromBody] string requestText)
        {
            GitHost host = await this.context.GitHost.FindAsync(hostId);

            if (host == null)
            {
                return this.NotFound(new ErrorResponse { Message = "Not a valid host id." });
            }

            Request request = this.requestSerialiser.FromString(requestText);

            return this.Ok(this.transferSerialiser.ToString(await this.HandleRequest(request, hostId, repositoryName)));
        }

        [HttpPut("/api/{hostId}/{repositoryName}/info/lfs/{objectId}")]
        public async Task<IActionResult> UploadFile(int hostId, string repositoryName, string objectId)
        {
            GitHost host = await this.context.GitHost.FindAsync(hostId);

            if (host == null)
            {
                return this.NotFound(new ErrorResponse { Message = "Not a valid host id." });
            }

            Stream body = this.HttpContext.Request.Body;

            string fileName = await this.fileManager.SaveFileForObjectId(repositoryName, objectId, body);

            await this.lfsClient.UploadFile(host, repositoryName, objectId);

            return this.Ok();
        }

        private async Task<Transfer> HandleRequest(Request request, int hostId, string repositoryName)
        {
            var response = new Transfer { Mode = TransferMode.Basic, Objects = new List<BatchObjectBase>() };
            foreach (RequestObject item in request.Objects)
            {
                GitLfsFile file = await this.context.LfsFiles.SingleOrDefaultAsync(x => x.ObjectId == item.ObjectId);

                if (file != null)
                {
                    file.Size = item.Size;
                }
                else
                {
                    this.context.LfsFiles.Add(new GitLfsFile { ObjectId = item.ObjectId, Size = item.Size });
                }

                var batchObject = new BatchObject { Size = item.Size, ObjectId = item.ObjectId };
                var location = new Uri(
                    $"{this.Request.Scheme}://{this.Request.Host}/api/{hostId}/{repositoryName}/info/lfs/{item.ObjectId}");

                batchObject.Actions =
                    new List<BatchObjectAction>
                        {
                            new BatchObjectAction
                                {
                                    HRef = location.AbsoluteUri,
                                    Mode = request.Operation == RequestMode.Upload
                                               ? BatchActionMode.Upload
                                               : BatchActionMode.Download
                                }
                        };

                response.Objects.Add(batchObject);
            }

            await this.context.SaveChangesAsync();

            return response;
        }
    }
}
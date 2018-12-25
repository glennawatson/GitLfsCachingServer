// <copyright file="ILfsClient.cs" company="Glenn Watson">
// Copyright (c) 2018 Glenn Watson. All rights reserved.
// See LICENSE file in the project root for full license information.
// </copyright>

namespace GitLfs.Client
{
    using System.IO;
    using System.Threading.Tasks;

    using GitLfs.Core;
    using GitLfs.Core.BatchRequest;
    using GitLfs.Core.BatchResponse;

    /// <summary>
    /// Represents a GIT LFS client.
    /// </summary>
    public interface ILfsClient
    {
        /// <summary>
        /// Downloads the file from the server.
        /// </summary>
        /// <param name="host">The git host where to download the file from.</param>
        /// <param name="repositoryName">The name of the repository where to download the file from.</param>
        /// <param name="objectId">The id of the object to download.</param>
        /// <param name="action">The action when doing batch operators.</param>
        /// <returns>A stream to the file.</returns>
        Task<Stream> DownloadFileAsync(GitHost host, string repositoryName, ObjectId objectId, BatchObjectAction action);

        /// <summary>
        /// Requests a batch transfer from the server.
        /// </summary>
        /// <param name="host">The host details where to upload the file.</param>
        /// <param name="repositoryName">The repository name.</param>
        /// <param name="request">The request details for the server.</param>
        /// <returns>The batch transfer returned from the server.</returns>
        Task<BatchTransfer> RequestBatchAsync(GitHost host, string repositoryName, BatchRequest request);

        /// <summary>
        /// Uploads a file to the server.
        /// </summary>
        /// <param name="action">The action to perform in regards to the batching.</param>
        /// <param name="stream">A stream to the file.</param>
        /// <returns>A task to monitor the progress.</returns>
        Task UploadFileAsync(BatchObjectAction action, Stream stream);

        /// <summary>
        /// Verify that a file action has been completed.
        /// </summary>
        /// <param name="host">The host to verify the action against.</param>
        /// <param name="repositoryName">The name of the repository.</param>
        /// <param name="objectId">The ID of the object to confirm.</param>
        /// <param name="action">The action to perform in regards to the batch object.</param>
        /// <returns>A task to monitor the progress.</returns>
        Task VerifyAsync(GitHost host, string repositoryName, ObjectId objectId, BatchObjectAction action);
    }
}
// <copyright file="ILfsClient.cs" company="Glenn Watson">
//    Copyright (C) 2017. Glenn Watson
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
        Task<Stream> DownloadFile(GitHost host, string repositoryName, ObjectId objectId, BatchObjectAction action);

        /// <summary>
        /// Requests a batch transfer from the server.
        /// </summary>
        /// <returns>The batch transfer returned from the server.</returns>
        /// <param name="host">The host details where to upload the file.</param>
        /// <param name="repositoryName">The repository name.</param>
        /// <param name="request">The request details for the server.</param>
        Task<BatchTransfer> RequestBatch(GitHost host, string repositoryName, BatchRequest request);

        Task UploadFile(BatchObjectAction action, Stream stream);

        Task Verify(GitHost host, string repositoryName, ObjectId objectId, BatchObjectAction action);
    }
}
// --------------------------------------------------------------------------------------------------------------------
// <copyright file="ILfsClient.cs" company="Glenn Watson">
//     Copyright (C) 2017. Glenn Watson
// </copyright>
// --------------------------------------------------------------------------------------------------------------------

namespace GitLfs.Client
{
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
        /// Uploads the selected file to the remote server.
        /// </summary>
        /// <param name="host">The host details where to upload the file.</param>
        /// <param name="repositoryName">The repository name.</param>
        /// <param name="objectId">The object that is referenced with the action.</param>
        /// <param name="action">The action to perform.</param>
        /// <returns>A task to monitor the progress.</returns>
        Task HandleBatchAction(GitHost host, string repositoryName, ObjectId objectId, BatchObjectAction action);

		/// <summary>
		/// Requests a batch transfer from the server.
		/// </summary>
		/// <returns>The batch transfer returned from the server.</returns>
		/// <param name="host">The host details where to upload the file.</param>
		/// <param name="repositoryName">The repository name.</param>
		/// <param name="request">The request details for the server.</param>
		Task<BatchTransfer> RequestBatch(GitHost host, string repositoryName, BatchRequest request);
    }
}
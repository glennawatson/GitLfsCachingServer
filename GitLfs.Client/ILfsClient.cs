// --------------------------------------------------------------------------------------------------------------------
// <copyright file="ILfsClient.cs" company="Glenn Watson">
//     Copyright (C) 2017. Glenn Watson
// </copyright>
// --------------------------------------------------------------------------------------------------------------------

namespace GitLfs.Client
{
    using System.IO;
    using System.Threading.Tasks;

    using GitLfs.Core;
    using GitLfs.Core.BatchRequest;

    /// <summary>
    /// Represents a GIT LFS client.
    /// </summary>
    public interface ILfsClient
    {
        /// <summary>
        /// Downloads a file.
        /// </summary>
        /// <param name="host">The host details where to download the file.</param>
        /// <param name="repositoryName">The repository name.</param>
        /// <param name="requestObject">The item to download.</param>
        /// <returns>The location on the local file system where the file is located.</returns>
        Task<Stream> DownloadFile(GitHost host, string repositoryName, BatchRequestObject requestObject);

        /// <summary>
        /// Uploads the selected file to the remote server.
        /// </summary>
        /// <param name="host">The host details where to upload the file.</param>
        /// <param name="repositoryName">The repository name.</param>
        /// <param name="requestObject">The item to upload.</param>
        /// <returns>A task to monitor the progress.</returns>
        Task UploadFile(GitHost host, string repositoryName, BatchRequestObject requestObject);
    }
}
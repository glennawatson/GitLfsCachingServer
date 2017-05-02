// --------------------------------------------------------------------------------------------------------------------
// <copyright file="ILfsClient.cs" company="Glenn Watson">
//   Copyright (C) 2017. Glenn Watson
// </copyright>
// <summary>
//   Represents a GIT LFS client.
// </summary>
// --------------------------------------------------------------------------------------------------------------------

namespace GitLfs.Client
{
    using System.Collections.Generic;
    using System.Threading.Tasks;

    /// <summary>
    /// Represents a GIT LFS client. 
    /// </summary>
    public interface ILfsClient
    {
        /// <summary>
        /// Downloads a file.
        /// </summary>
        /// <param name="objectId">The object id of the object.</param>
        /// <returns>The location on the local file system where the file is located.</returns>
        Task<string> DownloadFile(string objectId);

        /// <summary>
        /// Downloads several files.
        /// </summary>
        /// <param name="objectIds">The object ids of items to download.</param>
        /// <returns>The list of file paths.</returns>
        Task<IEnumerable<string>> DownloadFiles(IEnumerable<string> objectIds);
    }
}

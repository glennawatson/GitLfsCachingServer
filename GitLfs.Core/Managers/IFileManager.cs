// --------------------------------------------------------------------------------------------------------------------
// <copyright file="IFileManager.cs" company="Glenn Watson">
//     Copyright (C) 2017. Glenn Watson
// </copyright>
// --------------------------------------------------------------------------------------------------------------------
namespace GitLfs.Core.Managers
{
    using System.IO;
    using System.Threading.Tasks;

    public interface IFileManager
    {
        /// <summary>
        /// Gets the stream for a particular file. 
        /// </summary>
        /// <param name="repositoryName">The name of the repository.</param>
        /// <param name="objectId">The object id of the stream.</param>
        /// <returns>The contents of the file.</returns>
        Stream GetFileForObjectId(string repositoryName, string objectId);

        /// <summary>
        /// Saves a file in the local file store.
        /// </summary>
        /// <param name="repositoryName">The name of the repository.</param>
        /// <param name="objectId">The object id of the stream.</param>
        /// <param name="contents">The contents of the file.</param>
        /// <returns>A task to monitor the progress.</returns>
        Task<string> SaveFileForObjectId(string repositoryName, string objectId, Stream contents);

        /// <summary>
        /// Gets the file size of a object.
        /// </summary>
        /// <param name="repositoryName">The repository in the object.</param>
        /// <param name="objectId">The object id to check.</param>
        /// <returns>The file size if available, otherwise null.</returns>
        long? GetFileSize(string repositoryName, string objectId);
    }
}
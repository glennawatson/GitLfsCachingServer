// --------------------------------------------------------------------------------------------------------------------
// <copyright file="IFileManager.cs" company="Glenn Watson">
// Copyright (c) 2018 Glenn Watson. All rights reserved.
// See LICENSE file in the project root for full license information.
// </copyright>
// --------------------------------------------------------------------------------------------------------------------
namespace GitLfs.Core.File
{
    using System.IO;
    using System.Threading.Tasks;

    /// <summary>
    /// Handles files storage.
    /// </summary>
    public interface IFileManager
    {
        /// <summary>
        /// Gets the stream for a particular file.
        /// </summary>
        /// <param name="repositoryName">The name of the repository.</param>
        /// <param name="objectId">The object id of the stream.</param>
        /// <param name="location">Location of the file.</param>
        /// <param name="suffix">Suffix to add to the file name.</param>
        /// <returns>The contents of the file.</returns>
        Stream GetFileStream(string repositoryName, ObjectId objectId, FileLocation location, string suffix = null);

        /// <summary>
        /// Gets the string contents for a particular file.
        /// </summary>
        /// <param name="repositoryName">The name of the repository.</param>
        /// <param name="objectId">The object id of the stream.</param>
        /// <param name="location">Location of the file.</param>
        /// <param name="suffix">Suffix to add to the file name.</param>
        /// <returns>The contents of the file.</returns>
        Task<string> GetFileContentsAsync(string repositoryName, ObjectId objectId, FileLocation location, string suffix = null);

        /// <summary>
        /// Gets the file path for a particular file.
        /// </summary>
        /// <param name="repositoryName">The name of the repository.</param>
        /// <param name="objectId">The object id of the stream.</param>
        /// <param name="location">Location of the file.</param>
        /// <param name="suffix">Suffix to add to the file name.</param>
        /// <returns>The file path of the file.</returns>
        string GetFilePath(string repositoryName, ObjectId objectId, FileLocation location, string suffix = null);

        /// <summary>
        /// Saves a file in the local file store.
        /// </summary>
        /// <param name="repositoryName">The name of the repository.</param>
        /// <param name="objectId">The object id of the stream.</param>
        /// <param name="location">Location of the file.</param>
        /// <param name="contents">The contents of the file.</param>
        /// <param name="suffix">Suffix to add to the file name.</param>
        /// <returns>The file name.</returns>
        string SaveFile(string repositoryName, ObjectId objectId, FileLocation location, Stream contents, string suffix = null);

        /// <summary>
        /// Returns a stream that will save to the local file store as it is read.
        /// </summary>
        /// <param name="fileName">The file name to save. This is passed out.</param>
        /// <param name="repositoryName">The name of the repository.</param>
        /// <param name="objectId">The object id of the stream.</param>
        /// <param name="location">Location of the file.</param>
        /// <param name="contents">The contents of the file.</param>
        /// <param name="suffix">Suffix to add to the file name.</param>
        /// <returns>The file name.</returns>
        Stream SaveFile(out string fileName, string repositoryName, ObjectId objectId, FileLocation location, Stream contents, string suffix = null);

        /// <summary>
        /// Saves a file in the local file store.
        /// </summary>
        /// <param name="repositoryName">The name of the repository.</param>
        /// <param name="objectId">The object id of the stream.</param>
        /// <param name="location">Location of the file.</param>
        /// <param name="contents">The contents of the file.</param>
        /// <param name="suffix">Suffix to add to the file name.</param>
        /// <returns>A task to monitor the progress.</returns>
        Task<string> SaveFileAsync(string repositoryName, ObjectId objectId, FileLocation location, string contents, string suffix = null);

        /// <summary>
        /// Saves a file in the local file store.
        /// </summary>
        /// <param name="repositoryName">The name of the repository.</param>
        /// <param name="objectId">The object id of the stream.</param>
        /// <param name="location">Location of the file.</param>
        /// <param name="contents">The contents of the file.</param>
        /// <param name="suffix">Suffix to add to the file name.</param>
        /// <returns>The file name.</returns>
        Task<string> SaveFileAsync(string repositoryName, ObjectId objectId, FileLocation location, Stream contents, string suffix = null);

        /// <summary>
        /// Moves the file to permanent storage.
        /// </summary>
        /// <param name="repositoryName">The Repository name.</param>
        /// <param name="objectId">The Object identifier.</param>
        /// <param name="fromFileLocation">The location where the move the file from.</param>
        /// <param name="toFileLocation">The location to move the file to.</param>
        /// <param name="suffix">Suffix to add to the file name.</param>
        void MoveFile(string repositoryName, ObjectId objectId, FileLocation fromFileLocation, FileLocation toFileLocation, string suffix = null);

        /// <summary>
        /// Deletes the file.
        /// </summary>
        /// <param name="repositoryName">Repository name.</param>
        /// <param name="objectId">Object identifier.</param>
        /// <param name="location">Location of the file.</param>
        /// <param name="suffix">Suffix to add to the file name.</param>
        void DeleteFile(string repositoryName, ObjectId objectId, FileLocation location, string suffix = null);

        /// <summary>
        /// Determines if the file is in storage.
        /// </summary>
        /// <param name="repositoryName">The name of the repository.</param>
        /// <param name="objectId">The object id of the stream.</param>
        /// <param name="location">Location of the file.</param>
        /// <param name="matchSize">Determine the match based on matching the size.</param>
        /// <param name="suffix">Suffix to add to the file name.</param>
        /// <returns><c>true</c>, if file stored in storage, <c>false</c> otherwise.</returns>
        bool IsFileStored(string repositoryName, ObjectId objectId, FileLocation location, bool matchSize = false, string suffix = null);

        /// <summary>
        /// Gets the file size.
        /// </summary>
        /// <param name="repositoryName">The name of the repository.</param>
        /// <param name="objectId">The object id of the stream.</param>
        /// <param name="location">Location of the file.</param>
        /// <param name="suffix">Suffix to add to the file name.</param>
        /// <returns>The length of the file.</returns>
        long GetFileSize(string repositoryName, ObjectId objectId, FileLocation location, string suffix = null);
    }
}
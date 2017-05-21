// --------------------------------------------------------------------------------------------------------------------
// <copyright file="IFileManager.cs" company="Glenn Watson">
//     Copyright (C) 2017. Glenn Watson
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
		/// <returns>The contents of the file.</returns>
		Stream GetFileStream(string repositoryName, ObjectId objectId, FileLocation location);

		/// <summary>
		/// Gets the fiel path for a particular file. 
		/// </summary>
		/// <param name="repositoryName">The name of the repository.</param>
		/// <param name="objectId">The object id of the stream.</param>
		/// <param name="location">Location of the file.</param>
		/// <returns>The file path of the file.</returns>
		string GetFilePath(string repositoryName, ObjectId objectId, FileLocation location);

		/// <summary>
		/// Saves a file in the local file store.
		/// </summary>
		/// <param name="repositoryName">The name of the repository.</param>
		/// <param name="objectId">The object id of the stream.</param>
		/// <param name="location">Location of the file.</param>
		/// <param name="contents">The contents of the file.</param>
		/// <returns>A task to monitor the progress.</returns>
		string SaveFile(string repositoryName, ObjectId objectId, FileLocation location, Stream contents);

		/// <summary>
		/// Saves a file in the local file store.
		/// </summary>
		/// <param name="repositoryName">The name of the repository.</param>
		/// <param name="objectId">The object id of the stream.</param>
		/// <param name="location">Location of the file.</param>
		/// <param name="contents">The contents of the file.</param>
		/// <returns>A task to monitor the progress.</returns>
		Task<string> SaveFileAsync(string repositoryName, ObjectId objectId, FileLocation location, string contents);

        /// <summary>
        /// Moves the file to permanent storage.
        /// </summary>
        /// <param name="repositoryName">The Repository name.</param>
        /// <param name="objectId">The Object identifier.</param>
        /// <param name="from">The location where the move the file from.</param>
        /// <param name="to">The location to move the file to.</param>
        void MoveFile(string repositoryName, ObjectId objectId, FileLocation from, FileLocation to);

		/// <summary>
		/// Deletes the file.
		/// </summary>
		/// <param name="repositoryName">Repository name.</param>
		/// <param name="objectId">Object identifier.</param>
		/// <param name="location">Location of the file.</param>
		void DeleteFile(string repositoryName, ObjectId objectId, FileLocation location);

		/// <summary>
		/// Is the the file stored in storage?
		/// </summary>
		/// <param name="repositoryName">The name of the repository.</param>
		/// <param name="objectId">The object id of the stream.</param>
		/// <param name="location">Location of the file.</param>
		/// <returns><c>true</c>, if file stored in storage, <c>false</c> otherwise.</returns>
		bool IsFileStored(string repositoryName, ObjectId objectId, FileLocation location);
    }
}
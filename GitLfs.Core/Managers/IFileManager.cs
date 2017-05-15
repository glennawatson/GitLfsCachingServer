// --------------------------------------------------------------------------------------------------------------------
// <copyright file="IFileManager.cs" company="Glenn Watson">
//     Copyright (C) 2017. Glenn Watson
// </copyright>
// --------------------------------------------------------------------------------------------------------------------
namespace GitLfs.Core.Managers
{
    using System.IO;

    public interface IFileManager
    {
        /// <summary>
        /// Gets the stream for a particular file. 
        /// </summary>
        /// <param name="objectId">The object id of the stream.</param>
        /// <param name="repositoryName">The name of the repository.</param>
        /// <returns>The contents of the file.</returns>
        Stream GetFileForObjectId(string repositoryName, string objectId);
    }
}
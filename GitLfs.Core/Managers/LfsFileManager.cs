// --------------------------------------------------------------------------------------------------------------------
// <copyright file="LfsFileManager.cs" company="Glenn Watson">
//     Copyright (C) 2017. Glenn Watson
// </copyright>
// --------------------------------------------------------------------------------------------------------------------

namespace GitLfs.Core.Managers
{
    using System;
    using System.IO;

    public class LfsFileManager : IFileManager
    {
        private readonly string repositoryRoot;

        /// <summary>
        /// Initializes a new instance of the LfsFileManager class.
        /// </summary>
        /// <param name="repositoryRoot">The root directory where the LFS store is hosted.</param>
        public LfsFileManager(string repositoryRoot)
        {
            this.repositoryRoot = repositoryRoot;
        }

        /// <inheritdoc />
        public Stream GetFileForObjectId(string repositoryName, string objectId)
        {
            if (string.IsNullOrWhiteSpace(objectId))
            {
                throw new ArgumentException("Invalid object id", nameof(objectId));
            }

            string firstDirectory = Path.Combine(this.repositoryRoot, repositoryName, objectId.Substring(0, 2));

            if (Directory.Exists(firstDirectory) == false)
            {
                return null;
            }

            string secondDirectory = Path.Combine(firstDirectory, objectId.Substring(2, 2));

            if (Directory.Exists(secondDirectory))
            {
                return null;
            }

            string fileName = Path.Combine(secondDirectory, objectId);

            if (File.Exists(fileName) == false)
            {
                return null;
            }

            return new FileStream(fileName, FileMode.Open, FileAccess.Read);
        }
    }
}
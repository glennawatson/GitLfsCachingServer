// --------------------------------------------------------------------------------------------------------------------
// <copyright file="LfsFileManager.cs" company="Glenn Watson">
//     Copyright (C) 2017. Glenn Watson
// </copyright>
// --------------------------------------------------------------------------------------------------------------------

namespace GitLfs.Core.Managers
{
    using System;
    using System.IO;
    using System.Threading.Tasks;

    using Microsoft.AspNetCore.Hosting;

    public class LfsFileManager : IFileManager
    {
        private readonly IHostingEnvironment hostingEnvironment;

        /// <summary>
        /// Initializes a new instance of the LfsFileManager class.
        /// </summary>
        /// <param name="hostingEnvironment">Details about the hosting environment.</param>
        public LfsFileManager(IHostingEnvironment hostingEnvironment)
        {
            this.hostingEnvironment = hostingEnvironment;
        }

        /// <inheritdoc />
        public Stream GetFileForObjectId(string repositoryName, string objectId)
        {
            if (string.IsNullOrWhiteSpace(objectId))
            {
                throw new ArgumentException("Invalid object id", nameof(objectId));
            }

            string repositoryRoot = Path.Combine(this.hostingEnvironment.ContentRootPath, "lfs");

            if (Directory.Exists(repositoryRoot) == false)
            {
                return null;
            }

            Tuple<string, string, string> paths = this.GetDirectoriesAndFileNames(repositoryRoot, repositoryName, objectId);

            string fileName = paths.Item3;

            if (File.Exists(fileName) == false)
            {
                return null;
            }

            return new FileStream(fileName, FileMode.Open, FileAccess.Read);
        }

        /// <inheritdoc />
        public async Task<string> SaveFileForObjectId(string repositoryName, string objectId, Stream contents)
        {
            if (string.IsNullOrWhiteSpace(objectId))
            {
                throw new ArgumentException("Invalid object id", nameof(objectId));
            }

            string repositoryRoot = Path.Combine(this.hostingEnvironment.ContentRootPath, "lfs");

            if (Directory.Exists(repositoryRoot) == false)
            {
                Directory.CreateDirectory(repositoryRoot);
            }

            Tuple<string, string, string> paths = this.GetDirectoriesAndFileNames(repositoryRoot, repositoryName, objectId);

            if (Directory.Exists(paths.Item1) == false)
            {
                Directory.CreateDirectory(paths.Item1);
            }

            if (Directory.Exists(paths.Item2) == false)
            {
                Directory.CreateDirectory(paths.Item2);
            }

            using (var fileStream = new FileStream(paths.Item3, FileMode.Create, FileAccess.Write))
            {
                await contents.CopyToAsync(fileStream);

                return paths.Item3;
            }
        }

        /// <inheritdoc />
        public long? GetFileSize(string repositoryName, string objectId)
        {
            if (string.IsNullOrWhiteSpace(objectId))
            {
                throw new ArgumentException("Invalid object id", nameof(objectId));
            }

            string repositoryRoot = Path.Combine(this.hostingEnvironment.ContentRootPath, "lfs");

            if (Directory.Exists(repositoryRoot))
            {
                return null;
            }

            Tuple<string, string, string> paths = this.GetDirectoriesAndFileNames(repositoryRoot, repositoryName, objectId);

            string fileName = paths.Item3;

            if (File.Exists(fileName) == false)
            {
                return null;
            }

            var fileInfo = new FileInfo(fileName);
            return fileInfo.Length;
        }

        public Tuple<string, string, string> GetDirectoriesAndFileNames(string repositoryRoot, string repositoryName, string objectId)
        {
            string firstDirectory = Path.Combine(repositoryRoot, repositoryName, objectId.Substring(0, 2));

            string secondDirectory = Path.Combine(firstDirectory, objectId.Substring(2, 2));

            string fileName = Path.Combine(secondDirectory, objectId);

            return Tuple.Create(firstDirectory, secondDirectory, fileName);
        }
    }
}
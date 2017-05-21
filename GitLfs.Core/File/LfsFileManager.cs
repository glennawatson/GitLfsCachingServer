// --------------------------------------------------------------------------------------------------------------------
// <copyright file="LfsFileManager.cs" company="Glenn Watson">
//     Copyright (C) 2017. Glenn Watson
// </copyright>
// --------------------------------------------------------------------------------------------------------------------

namespace GitLfs.Core.File
{
    using System;
    using System.Collections.Generic;
    using System.IO;
    using System.Threading.Tasks;

    using Microsoft.AspNetCore.Hosting;

    public class LfsFileManager : IFileManager
    {
        private const string DirectoryPrefix = "lfs";
        private static readonly IDictionary<FileLocation, string> Prefixes = new Dictionary<FileLocation, string>
        {
            { FileLocation.Temporary, "temp" },
            { FileLocation.Permenant, "perm" },
            { FileLocation.Metadata, "meta" }
        };

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
		public Stream GetFileStream(string repositoryName, ObjectId objectId, FileLocation location)
		{
            if (this.IsFileStored(repositoryName, objectId, location) == false)
            {
                return null;
            }

            var path = this.GetDirectoriesAndFileNames(repositoryName, objectId, location).Item2;
            return new FileStream(path, FileMode.Open, FileAccess.Read);
		}

		/// <inheritdoc />
		public string GetFilePath(string repositoryName, ObjectId objectId, FileLocation location)
        {
            if (this.IsFileStored(repositoryName, objectId, location) == false)
            {
                return null;
            }

            var path = this.GetDirectoriesAndFileNames(repositoryName, objectId, location).Item2;
            return path;
        }

		/// <inheritdoc />
		public string SaveFile(string repositoryName, ObjectId objectId, FileLocation location, Stream contents)
		{
			var temporaryPath = this.GetDirectoriesAndFileNames(repositoryName, objectId, location);

            Directory.CreateDirectory(temporaryPath.Item1);

			using (var fileStream = new FileStream(temporaryPath.Item2, FileMode.Create, FileAccess.Write))
			{
				contents.CopyTo(fileStream);

				return temporaryPath.Item2;
			}
		}

		/// <inheritdoc />
		public async Task<string> SaveFileAsync(string repositoryName, ObjectId objectId, FileLocation location, string contents)
		{
			var temporaryPath = this.GetDirectoriesAndFileNames(repositoryName, objectId, location);

			Directory.CreateDirectory(temporaryPath.Item1);

            using (var fileStream = new StreamWriter(new FileStream(temporaryPath.Item2, FileMode.Create, FileAccess.Write)))
			{
                await fileStream.WriteAsync(contents);

				return temporaryPath.Item2;
			}
		}
		
        /// <inheritdoc />
        public void DeleteFile(string repositoryName, ObjectId objectId, FileLocation location)
        {
			var path = this.GetDirectoriesAndFileNames(repositoryName, objectId, location);

            if (File.Exists(path.Item2) == false)
            {
                return;
            }

            File.Delete(path.Item2);

            var currentDirectory = Path.GetDirectoryName(path.Item2);

			for (int i = 0; i < 2; ++i)
            {
                if (Directory.GetFileSystemEntries(currentDirectory).Length == 0)
				{
                    Directory.Delete(currentDirectory);
				}
                else
                {
                    break;
                }

                currentDirectory = Path.GetDirectoryName(currentDirectory);
			}
		}

		/// <inheritdoc />
		public void MoveFile(string repositoryName, ObjectId objectId, FileLocation from, FileLocation to)
		{
			var temporaryPath = this.GetDirectoriesAndFileNames(repositoryName, objectId, from);
			var permenantPath = this.GetDirectoriesAndFileNames(repositoryName, objectId, to);

            Directory.CreateDirectory(permenantPath.Item1);

            if (File.Exists(permenantPath.Item2))
            {
                return;
            }

            File.Move(temporaryPath.Item2, permenantPath.Item2);

            File.Delete(temporaryPath.Item2);
		}

		/// <inheritdoc />
		public bool IsFileStored(string repositoryName, ObjectId objectId, FileLocation location)
        {
            Tuple<string, string> paths = this.GetDirectoriesAndFileNames(repositoryName, objectId, location);

			if (Directory.Exists(paths.Item1) == false)
			{
                return false;
			}

            if (File.Exists(paths.Item2) == false)
            {
                return false;
            }

            return true;
		}

		public Tuple<string, string> GetDirectoriesAndFileNames(string repositoryName, ObjectId objectId, FileLocation location)
		{
			string repositoryRoot = Path.Combine(this.hostingEnvironment.ContentRootPath, DirectoryPrefix);

            repositoryRoot = Path.Combine(repositoryRoot, Prefixes[location]);

			string firstDirectory = Path.Combine(repositoryRoot, repositoryName, objectId.Hash.Substring(0, 2));

			string secondDirectory = Path.Combine(firstDirectory, objectId.Hash.Substring(2, 2));

			string fileName = Path.Combine(secondDirectory, objectId.Hash);

			return Tuple.Create(secondDirectory, fileName);
		}
    }
}
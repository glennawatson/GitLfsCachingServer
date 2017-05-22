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
    using System.Text;

    /// <summary>
    /// Manages the files inside a LFS store.
    /// </summary>
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
        public Stream GetFileStream(string repositoryName, ObjectId objectId, FileLocation location, string suffix = null)
        {
            if (this.IsFileStored(repositoryName, objectId, location, suffix) == false)
            {
                return null;
            }

            string path = this.GetDirectoriesAndFileNames(repositoryName, objectId, location, suffix).Item2;
            return new FileStream(path, FileMode.Open, FileAccess.Read);
        }

        /// <inheritdoc />
        public async Task<string> GetFileContentsAsync(string repositoryName, ObjectId objectId, FileLocation location, string suffix = null)
        {
            using (var streamer = new StreamReader(this.GetFileStream(repositoryName, objectId, location, suffix)))
            {
                return await streamer.ReadToEndAsync();
            }
        }

        /// <inheritdoc />
        public string GetFilePath(string repositoryName, ObjectId objectId, FileLocation location, string suffix = null)
        {
            if (this.IsFileStored(repositoryName, objectId, location, suffix) == false)
            {
                return null;
            }

            string path = this.GetDirectoriesAndFileNames(repositoryName, objectId, location, suffix).Item2;
            return path;
        }

        /// <inheritdoc />
        public string SaveFile(string repositoryName, ObjectId objectId, FileLocation location, Stream contents, string suffix = null)
        {
            Tuple<string, string> temporaryPath = this.GetDirectoriesAndFileNames(repositoryName, objectId, location, suffix);

            Directory.CreateDirectory(temporaryPath.Item1);

            using (FileStream fileStream = new FileStream(temporaryPath.Item2, FileMode.Create, FileAccess.Write))
            {
                contents.CopyTo(fileStream);

                return temporaryPath.Item2;
            }
        }

        /// <inheritdoc />
        public async Task<string> SaveFileAsync(string repositoryName, ObjectId objectId, FileLocation location, string contents, string suffix = null)
        {
            Tuple<string, string> temporaryPath = this.GetDirectoriesAndFileNames(repositoryName, objectId, location, suffix);

            Directory.CreateDirectory(temporaryPath.Item1);

            using (StreamWriter fileStream = new StreamWriter(new FileStream(temporaryPath.Item2, FileMode.Create, FileAccess.Write)))
            {
                await fileStream.WriteAsync(contents);

                return temporaryPath.Item2;
            }
        }

        /// <inheritdoc />
        public async Task<string> SaveFileAsync(string repositoryName, ObjectId objectId, FileLocation location, Stream contents, string suffix = null)
        {
            Tuple<string, string> temporaryPath = this.GetDirectoriesAndFileNames(repositoryName, objectId, location, suffix);

            Directory.CreateDirectory(temporaryPath.Item1);

            using (FileStream fileStream = new FileStream(temporaryPath.Item2, FileMode.Create, FileAccess.Write))
            {
                await contents.CopyToAsync(fileStream);

                return temporaryPath.Item2;
            }
        }

        /// <inheritdoc />
        public void DeleteFile(string repositoryName, ObjectId objectId, FileLocation location, string suffix = null)
        {
            Tuple<string, string> path = this.GetDirectoriesAndFileNames(repositoryName, objectId, location, suffix);

            if (File.Exists(path.Item2) == false)
            {
                return;
            }

            File.Delete(path.Item2);

            string currentDirectory = Path.GetDirectoryName(path.Item2);

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
        public void MoveFile(string repositoryName, ObjectId objectId, FileLocation from, FileLocation to, string suffix = null)
        {
            Tuple<string, string> temporaryPath = this.GetDirectoriesAndFileNames(repositoryName, objectId, from, suffix);
            Tuple<string, string> permenantPath = this.GetDirectoriesAndFileNames(repositoryName, objectId, to, suffix);

            Directory.CreateDirectory(permenantPath.Item1);

            if (File.Exists(permenantPath.Item2))
            {
                return;
            }

            File.Move(temporaryPath.Item2, permenantPath.Item2);

            File.Delete(temporaryPath.Item2);
        }

        /// <inheritdoc />
        public bool IsFileStored(string repositoryName, ObjectId objectId, FileLocation location, string suffix = null)
        {
            Tuple<string, string> paths = this.GetDirectoriesAndFileNames(repositoryName, objectId, location, suffix);

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

        private Tuple<string, string> GetDirectoriesAndFileNames(string repositoryName, ObjectId objectId, FileLocation location, string suffix = null)
        {
            string repositoryRoot = Path.Combine(this.hostingEnvironment.ContentRootPath, DirectoryPrefix);

            repositoryRoot = Path.Combine(repositoryRoot, Prefixes[location]);

            string firstDirectory = Path.Combine(repositoryRoot, repositoryName, objectId.Hash.Substring(0, 2));

            string secondDirectory = Path.Combine(firstDirectory, objectId.Hash.Substring(2, 2));

            StringBuilder fileName = new StringBuilder(Path.Combine(secondDirectory, objectId.Hash));

            if (suffix != null)
            {
                fileName.Append($"-{suffix}");
            }

            return Tuple.Create(secondDirectory, fileName.ToString());
        }
    }
}
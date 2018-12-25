// --------------------------------------------------------------------------------------------------------------------
// <copyright file="LfsFileManager.cs" company="Glenn Watson">
// Copyright (c) 2018 Glenn Watson. All rights reserved.
// See LICENSE file in the project root for full license information.
// </copyright>
// --------------------------------------------------------------------------------------------------------------------

namespace GitLfs.Core.File
{
    using System;
    using System.Collections.Generic;
    using System.IO;
    using System.Text;
    using System.Threading.Tasks;
    using GitLfs.Core.ErrorHandling;
    using Microsoft.AspNetCore.Hosting;

    /// <summary>
    /// Manages the files inside a LFS store.
    /// </summary>
    public class LfsFileManager : IFileManager
    {
        private const string DirectoryPrefix = "lfs";
        private static readonly IDictionary<FileLocation, string> Prefixes = new Dictionary<FileLocation, string>
                                                                                 {
                                                                                     { FileLocation.Temporary, "temp" },
                                                                                     { FileLocation.Permanent, "perm" },
                                                                                     { FileLocation.Metadata, "meta" }
                                                                                 };

        private readonly IHostingEnvironment hostingEnvironment;

        /// <summary>
        /// Initializes a new instance of the <see cref="LfsFileManager"/> class.
        /// </summary>
        /// <param name="hostingEnvironment">Details about the hosting environment.</param>
        public LfsFileManager(IHostingEnvironment hostingEnvironment)
        {
            this.hostingEnvironment = hostingEnvironment;
        }

        /// <inheritdoc />
        public Stream GetFileStream(string repositoryName, ObjectId objectId, FileLocation location, string suffix = null)
        {
            try
            {
                if (!this.IsFileStored(repositoryName, objectId, location, false, suffix))
                {
                    return null;
                }

                string path = this.GetDirectoriesAndFileNames(repositoryName, objectId, location, suffix).Item2;
                return new FileStream(path, FileMode.Open, FileAccess.Read);
            }
            catch (IOException ex)
            {
                throw new ErrorResponseException(new ErrorResponse() { Message = ex.Message }, 500);
            }
        }

        /// <inheritdoc />
        public async Task<string> GetFileContentsAsync(string repositoryName, ObjectId objectId, FileLocation location, string suffix = null)
        {
            using (var streamer = new StreamReader(this.GetFileStream(repositoryName, objectId, location, suffix)))
            {
                return await streamer.ReadToEndAsync().ConfigureAwait(false);
            }
        }

        /// <inheritdoc />
        public string GetFilePath(string repositoryName, ObjectId objectId, FileLocation location, string suffix = null)
        {
            if (!this.IsFileStored(repositoryName, objectId, location, false, suffix))
            {
                return null;
            }

            return this.GetDirectoriesAndFileNames(repositoryName, objectId, location, suffix).Item2;
        }

        /// <inheritdoc />
        public string SaveFile(string repositoryName, ObjectId objectId, FileLocation location, Stream contents, string suffix = null)
        {
            var (directory, fileName) = this.GetDirectoriesAndFileNames(repositoryName, objectId, location, suffix);

            try
            {
                Directory.CreateDirectory(directory);

                using (FileStream fileStream = new FileStream(fileName, FileMode.Create, FileAccess.Write))
                {
                    contents.CopyTo(fileStream);

                    return fileName;
                }
            }
            catch (IOException ex)
            {
                throw new ErrorResponseException(new ErrorResponse() { Message = ex.Message }, 500);
            }
        }

        /// <inheritdoc />
        public Stream SaveFile(out string fileName, string repositoryName, ObjectId objectId, FileLocation location, Stream contents, string suffix = null)
        {
            var (directory, inputFileName) = this.GetDirectoriesAndFileNames(repositoryName, objectId, location, suffix);

            try
            {
                Directory.CreateDirectory(directory);
                fileName = inputFileName;
                return new CachingStream(contents, fileName);
            }
            catch (IOException ex)
            {
                throw new ErrorResponseException(new ErrorResponse() { Message = ex.Message }, 500);
            }
        }

        /// <inheritdoc />
        public async Task<string> SaveFileAsync(string repositoryName, ObjectId objectId, FileLocation location, string contents, string suffix = null)
        {
            var (directory, fileName) = this.GetDirectoriesAndFileNames(repositoryName, objectId, location, suffix);

            try
            {
                Directory.CreateDirectory(directory);

                using (StreamWriter fileStream = new StreamWriter(new FileStream(fileName, FileMode.Create, FileAccess.Write)))
                {
                    await fileStream.WriteAsync(contents).ConfigureAwait(false);

                    return fileName;
                }
            }
            catch (IOException ex)
            {
                throw new ErrorResponseException(new ErrorResponse() { Message = ex.Message }, 500);
            }
        }

        /// <inheritdoc />
        public async Task<string> SaveFileAsync(string repositoryName, ObjectId objectId, FileLocation location, Stream contents, string suffix = null)
        {
            var (directory, fileName) = this.GetDirectoriesAndFileNames(repositoryName, objectId, location, suffix);

            try
            {
                Directory.CreateDirectory(directory);

                using (FileStream fileStream = new FileStream(fileName, FileMode.Create, FileAccess.Write))
                {
                    await contents.CopyToAsync(fileStream).ConfigureAwait(false);

                    return fileName;
                }
            }
            catch (IOException ex)
            {
                throw new ErrorResponseException(new ErrorResponse() { Message = ex.Message }, 500);
            }
        }

        /// <inheritdoc />
        public void DeleteFile(string repositoryName, ObjectId objectId, FileLocation location, string suffix = null)
        {
            var (directory, fileName) = this.GetDirectoriesAndFileNames(repositoryName, objectId, location, suffix);

            if (!File.Exists(fileName))
            {
                return;
            }

            try
            {
                File.Delete(fileName);

                CleanDirectoryIfEmpty(directory);
            }
            catch (IOException ex)
            {
                throw new ErrorResponseException(new ErrorResponse() { Message = ex.Message }, 500);
            }
        }

        /// <inheritdoc />
        public void MoveFile(string repositoryName, ObjectId objectId, FileLocation fromFileLocation, FileLocation toFileLocation, string suffix = null)
        {
            var (tempDirectory, tempFileName) = this.GetDirectoriesAndFileNames(repositoryName, objectId, fromFileLocation, suffix);
            var (permanentDirectory, permanentFileName) = this.GetDirectoriesAndFileNames(repositoryName, objectId, toFileLocation, suffix);

            try
            {
                Directory.CreateDirectory(permanentDirectory);

                if (File.Exists(permanentDirectory))
                {
                    return;
                }

                File.Move(tempFileName, permanentFileName);

                CleanDirectoryIfEmpty(tempDirectory);
            }
            catch (IOException ex)
            {
                throw new ErrorResponseException(new ErrorResponse { Message = ex.Message }, 500);
            }
        }

        /// <inheritdoc />
        public bool IsFileStored(string repositoryName, ObjectId objectId, FileLocation location, bool matchSize = false, string suffix = null)
        {
            var (directory, fileName) = this.GetDirectoriesAndFileNames(repositoryName, objectId, location, suffix);

            if (!Directory.Exists(directory))
            {
                return false;
            }

            if (!File.Exists(fileName))
            {
                return false;
            }

            if (matchSize)
            {
                var fileInfo = new FileInfo(fileName);

                if (fileInfo.Length != objectId.Size)
                {
                    return false;
                }
            }

            return true;
        }

        /// <inheritdoc />
        public long GetFileSize(string repositoryName, ObjectId objectId, FileLocation location, string suffix = null)
        {
            var (directory, fileName) = this.GetDirectoriesAndFileNames(repositoryName, objectId, location, suffix);

            if (!Directory.Exists(directory))
            {
                return 0;
            }

            if (!File.Exists(fileName))
            {
                return 0;
            }

            return new FileInfo(fileName).Length;
        }

        private static void CleanDirectoryIfEmpty(string path)
        {
            string currentDirectory = Path.GetDirectoryName(path);

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

        private (string directory, string fileName) GetDirectoriesAndFileNames(string repositoryName, ObjectId objectId, FileLocation location, string suffix = null)
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

            return (secondDirectory, fileName.ToString());
        }
    }
}
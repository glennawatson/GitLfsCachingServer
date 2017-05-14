// --------------------------------------------------------------------------------------------------------------------
// <copyright file="FileCachingLfsClient.cs" company="Glenn Watson">
//   Copyright (C) Glenn Watson
// </copyright>
// <summary>
//   Defines the FileCachingLfsClient type.
// </summary>
// --------------------------------------------------------------------------------------------------------------------

namespace GitLfs.Client
{
    using System;
    using System.Collections.Generic;
    using System.Threading.Tasks;

    public class FileCachingLfsClient : ILfsClient
    {
        /// <inheritdoc />
        public Task<string> DownloadFile(string objectId)
        {
            throw new NotImplementedException();
        }

        /// <inheritdoc />
        public Task<IEnumerable<string>> DownloadFiles(IEnumerable<string> objectIds)
        {
            throw new NotImplementedException();
        }
    }
}
// --------------------------------------------------------------------------------------------------------------------
// <copyright file="FileCachingLfsClient.cs" company="Glenn Watson">
//     Copyright (C) 2017. Glenn Watson
// </copyright>
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
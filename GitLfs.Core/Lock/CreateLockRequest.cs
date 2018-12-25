// <copyright file="CreateLockRequest.cs" company="Glenn Watson">
// Copyright (c) 2018 Glenn Watson. All rights reserved.
// See LICENSE file in the project root for full license information.
// </copyright>

namespace GitLfs.Core.Lock
{
    /// <summary>
    /// A request to lock a particular file.
    /// </summary>
    public class CreateLockRequest
    {
        /// <summary>
        /// Gets or sets the path to the file to lock.
        /// </summary>
        public string Path { get; set; }
    }
}
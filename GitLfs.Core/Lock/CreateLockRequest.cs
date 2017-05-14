// --------------------------------------------------------------------------------------------------------------------
// <copyright file="CreateLockRequest.cs" company="Glenn Watson">
//     Copyright (C) 2017. Glenn Watson
// </copyright>
// --------------------------------------------------------------------------------------------------------------------

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
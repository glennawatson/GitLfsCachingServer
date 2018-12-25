// <copyright file="StatusCodeException.cs" company="Glenn Watson">
// Copyright (c) 2018 Glenn Watson. All rights reserved.
// See LICENSE file in the project root for full license information.
// </copyright>

namespace GitLfs.Core
{
    using System.Net;

    /// <summary>
    /// Exception that occurs during handling file caching to the server.
    /// </summary>
    public class StatusCodeException : LfsException
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="StatusCodeException"/> class.
        /// </summary>
        /// <param name="statusCode">The error status code to show.</param>
        /// <param name="message">A human friendly message.</param>
        public StatusCodeException(HttpStatusCode statusCode, string message)
            : base(message)
        {
            this.StatusCode = (int)statusCode;
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="StatusCodeException"/> class.
        /// </summary>
        /// <param name="statusCode">The error status code to show.</param>
        /// <param name="message">A human friendly message.</param>
        public StatusCodeException(int? statusCode, string message)
            : base(message)
        {
            this.StatusCode = statusCode;
        }

        /// <summary>
        /// Gets the HTTP status code.
        /// </summary>
        public int? StatusCode { get; }
    }
}
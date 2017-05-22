// <copyright file="StatusCodeException.cs" company="Glenn Watson">
//    Copyright (C) 2017. Glenn Watson
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
        /// Initializes a new instance of the ClientException class.
        /// </summary>
        /// <param name="statusCode">The error status code to show.</param>
        /// <param name="message">A human friendly message.</param>
        public StatusCodeException(HttpStatusCode statusCode, string message)
            : base(message)
        {
            this.StatusCode = (int)statusCode;
        }

        /// <summary>
        /// Initializes a new instance of the ClientException class.
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
// --------------------------------------------------------------------------------------------------------------------
// <copyright file="ClientException.cs" company="Glenn Watson">
//     Copyright (C) 2017. Glenn Watson
// </copyright>
// --------------------------------------------------------------------------------------------------------------------

namespace GitLfs.Client
{
    using System;
    using System.Net;

    /// <summary>
    /// Exception that occurs during handling file caching to the server. 
    /// </summary>
    public class ClientException : Exception
    {
        /// <summary>
        /// Initializes a new instance of the ClientException class. 
        /// </summary>
        /// <param name="statusCode">The error status code to show.</param>
        /// <param name="message">A human friendly message.</param>
        public ClientException(HttpStatusCode statusCode, string message)
            : base(message)
        {
            this.StatusCode = (int)statusCode;
        }

        /// <summary>
        /// Initializes a new instance of the ClientException class. 
        /// </summary>
        /// <param name="statusCode">The error status code to show.</param>
        /// <param name="message">A human friendly message.</param>
        public ClientException(int statusCode, string message)
            : base(message)
        {
            this.StatusCode = statusCode;
        }

        /// <summary>
        /// Gets the HTTP status code.
        /// </summary>
        public int StatusCode { get; }
    }
}
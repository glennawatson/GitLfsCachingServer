// <copyright file="ErrorResponseException.cs" company="Glenn Watson">
// Copyright (c) 2018 Glenn Watson. All rights reserved.
// See LICENSE file in the project root for full license information.
// </copyright>

namespace GitLfs.Core
{
    using GitLfs.Core.ErrorHandling;

    /// <summary>
    /// An exception that includes a error response.
    /// </summary>
    public class ErrorResponseException : StatusCodeException
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="ErrorResponseException"/> class.
        /// </summary>
        /// <param name="response">The response.</param>
        /// <param name="statusCode">The status code returned by the server.</param>
        public ErrorResponseException(ErrorResponse response, int? statusCode = null)
            : base(statusCode, response.Message)
        {
            this.ErrorResponse = response;
        }

        /// <summary>
        /// Gets the error response.
        /// </summary>
        public ErrorResponse ErrorResponse { get; }
    }
}
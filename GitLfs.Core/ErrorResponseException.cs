// <copyright file="ErrorResponseException.cs" company="Glenn Watson">
// Copyright (c) 2018 Glenn Watson. All rights reserved.
// See LICENSE file in the project root for full license information.
// </copyright>

namespace GitLfs.Core;

using System.Net;

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
        ErrorResponse = response;
    }

    /// <summary>
    /// Initializes a new instance of the <see cref="ErrorResponseException"/> class.
    /// </summary>
    /// <param name="statusCode">The status code.</param>
    /// <param name="message">The message.</param>
    public ErrorResponseException(HttpStatusCode statusCode, string message)
        : base(statusCode, message)
    {
    }

    /// <summary>
    /// Initializes a new instance of the <see cref="ErrorResponseException"/> class.
    /// </summary>
    /// <param name="statusCode">The status code.</param>
    /// <param name="message">The message.</param>
    public ErrorResponseException(int? statusCode, string message)
        : base(statusCode, message)
    {
    }

    /// <summary>
    /// Initializes a new instance of the <see cref="ErrorResponseException"/> class.
    /// </summary>
    public ErrorResponseException()
    {
    }

    /// <summary>
    /// Initializes a new instance of the <see cref="ErrorResponseException"/> class.
    /// </summary>
    /// <param name="message">The message.</param>
    public ErrorResponseException(string message)
        : base(message)
    {
    }

    /// <summary>
    /// Initializes a new instance of the <see cref="ErrorResponseException"/> class.
    /// </summary>
    /// <param name="message">The message.</param>
    /// <param name="innerException">The inner exception.</param>
    public ErrorResponseException(string message, Exception innerException)
        : base(message, innerException)
    {
    }

    /// <summary>
    /// Gets the error response.
    /// </summary>
    public ErrorResponse ErrorResponse { get; }
}

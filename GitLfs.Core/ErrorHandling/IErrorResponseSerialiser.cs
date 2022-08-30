// <copyright file="IErrorResponseSerialiser.cs" company="Glenn Watson">
// Copyright (c) 2018 Glenn Watson. All rights reserved.
// See LICENSE file in the project root for full license information.
// </copyright>

namespace GitLfs.Core.ErrorHandling;

/// <summary>
/// A serializer which will convert a error response to and from a LFS server.
/// </summary>
public interface IErrorResponseSerialiser
{
    /// <summary>
    /// Create a transfer object from a string.
    /// </summary>
    /// <param name="value">The string value to convert from.</param>
    /// <returns>The transfer object.</returns>
    ErrorResponse FromString(string value);

    /// <summary>
    /// Create a string value from a transfer.
    /// </summary>
    /// <param name="transfer">The transfer object to convert from.</param>
    /// <returns>The string value representing the transfer object.</returns>
    string ToString(ErrorResponse transfer);
}

// <copyright file="BatchObjectError.cs" company="Glenn Watson">
// Copyright (c) 2018 Glenn Watson. All rights reserved.
// See LICENSE file in the project root for full license information.
// </copyright>

namespace GitLfs.Core.BatchResponse;

/// <summary>
/// Represents a error when performing a batch operation with LFS.
/// </summary>
public class BatchObjectError : IBatchObject
{
    /// <summary>
    /// Gets or sets the error code provided by the server.
    /// </summary>
    public int ErrorCode { get; set; }

    /// <summary>
    /// Gets or sets any additional error message provided by the server.
    /// </summary>
    public string ErrorMessage { get; set; }

    /// <inheritdoc />
    public ObjectId Id { get; set; }
}

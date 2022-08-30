// <copyright file="IBatchObject.cs" company="Glenn Watson">
// Copyright (c) 2018 Glenn Watson. All rights reserved.
// See LICENSE file in the project root for full license information.
// </copyright>

namespace GitLfs.Core.BatchResponse;

/// <summary>
/// A batch object returned or sent to a LFS server.
/// </summary>
public interface IBatchObject
{
    /// <summary>
    /// Gets or sets the id of the batch object.
    /// </summary>
    ObjectId Id { get; set; }
}

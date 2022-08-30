// <copyright file="BatchRequest.cs" company="Glenn Watson">
// Copyright (c) 2018 Glenn Watson. All rights reserved.
// See LICENSE file in the project root for full license information.
// </copyright>

namespace GitLfs.Core.BatchRequest;

using System.Collections.Generic;

/// <summary>
/// A batch request to a LFS server.
/// </summary>
public class BatchRequest
{
    /// <summary>
    /// Gets or sets the objects associated with the batch request.
    /// </summary>
    public IList<ObjectId> Objects { get; set; }

    /// <summary>
    /// Gets or sets the batch request operation we want to perform.
    /// </summary>
    public BatchRequestMode Operation { get; set; }

    /// <summary>
    /// Gets or sets the transfer modes for the objects.
    /// </summary>
    public IList<TransferMode> Transfers { get; set; }
}

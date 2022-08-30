// <copyright file="BatchActionMode.cs" company="Glenn Watson">
// Copyright (c) 2018 Glenn Watson. All rights reserved.
// See LICENSE file in the project root for full license information.
// </copyright>

namespace GitLfs.Core.BatchResponse;

/// <summary>
/// The action mode of a batch request.
/// </summary>
public enum BatchActionMode
{
    /// <summary>
    /// The batch mode is to upload files.
    /// </summary>
    Upload,

    /// <summary>
    /// The batch mode is to download files.
    /// </summary>
    Download,

    /// <summary>
    /// The batch mode is to verify the LFS status on the server.
    /// </summary>
    Verify,
}

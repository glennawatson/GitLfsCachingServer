// <copyright file="FileLocation.cs" company="Glenn Watson">
// Copyright (c) 2018 Glenn Watson. All rights reserved.
// See LICENSE file in the project root for full license information.
// </copyright>

namespace GitLfs.Core.File;

/// <summary>
/// The location where the file should be located. Used by the LFS system.
/// </summary>
public enum FileLocation
{
    /// <summary>
    /// The file is located temporarily.
    /// </summary>
    Temporary,

    /// <summary>
    /// The file should be stored permanently.
    /// </summary>
    Permanent,

    /// <summary>
    /// The file is just metadata.
    /// </summary>
    Metadata,
}

// <copyright file="BatchRequestMode.cs" company="Glenn Watson">
// Copyright (c) 2018 Glenn Watson. All rights reserved.
// See LICENSE file in the project root for full license information.
// </copyright>

namespace GitLfs.Core.BatchRequest
{
    using System.Runtime.Serialization;

    /// <summary>
    /// Gets the batch request mode we are doing with a batch request.
    /// </summary>
    public enum BatchRequestMode
    {
        /// <summary>
        /// The mode of the batch request is to upload files.
        /// </summary>
        [EnumMember(Value = "upload")]
        Upload,

        /// <summary>
        /// The mode of the batch request is to download files.
        /// </summary>
        [EnumMember(Value = "download")]
        Download
    }
}
// <copyright file="TransferMode.cs" company="Glenn Watson">
// Copyright (c) 2018 Glenn Watson. All rights reserved.
// See LICENSE file in the project root for full license information.
// </copyright>

namespace GitLfs.Core
{
    using System.Runtime.Serialization;

    /// <summary>
    /// The mode to use when transferring files.
    /// </summary>
    public enum TransferMode
    {
        /// <summary>
        /// A value indicating that we are going to use the basic HTTP/S method of transferring files.
        /// </summary>
        [EnumMember(Value = "basic")]
        Basic
    }
}
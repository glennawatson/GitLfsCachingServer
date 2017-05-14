// --------------------------------------------------------------------------------------------------------------------
// <copyright file="TransferMode.cs" company="Glenn Watson">
//     Copyright (C) 2017. Glenn Watson
// </copyright>
// --------------------------------------------------------------------------------------------------------------------

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
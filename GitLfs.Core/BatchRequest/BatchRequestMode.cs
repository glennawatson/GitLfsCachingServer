// --------------------------------------------------------------------------------------------------------------------
// <copyright file="RequestMode.cs" company="Glenn Watson">
//     Copyright (C) 2017. Glenn Watson
// </copyright>
// --------------------------------------------------------------------------------------------------------------------

namespace GitLfs.Core.BatchRequest
{
    using System.Runtime.Serialization;

    public enum BatchRequestMode
    {
        [EnumMember(Value = "upload")]
        Upload,

        [EnumMember(Value = "download")]
        Download
    }
}
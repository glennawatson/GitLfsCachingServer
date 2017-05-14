// --------------------------------------------------------------------------------------------------------------------
// <copyright file="RequestObject.cs" company="Glenn Watson">
//     Copyright (C) 2017. Glenn Watson
// </copyright>
// --------------------------------------------------------------------------------------------------------------------

namespace GitLfs.Core.BatchRequest
{
    using Newtonsoft.Json;

    public class RequestObject
    {
        [JsonProperty(PropertyName = "oid")]
        public string ObjectId { get; set; }

        public long Size { get; set; }
    }
}
// --------------------------------------------------------------------------------------------------------------------
// <copyright file="BatchObject.cs" company="Glenn Watson">
//     Copyright (C) 2017. Glenn Watson
// </copyright>
// --------------------------------------------------------------------------------------------------------------------

namespace GitLfs.Core.BatchResponse
{
    using System.Collections.Generic;

    using Newtonsoft.Json;

    public class BatchObject
    {
        public IList<BatchObjectAction> Actions { get; set; }

        public bool? Authenticated { get; set; }

        [JsonProperty(PropertyName = "oid")]
        public string ObjectId { get; set; }

        public long Size { get; set; }
    }
}
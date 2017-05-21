// --------------------------------------------------------------------------------------------------------------------
// <copyright file="Request.cs" company="Glenn Watson">
//     Copyright (C) 2017. Glenn Watson
// </copyright>
// --------------------------------------------------------------------------------------------------------------------

namespace GitLfs.Core.BatchRequest
{
    using System.Collections.Generic;

    public class BatchRequest
    {
        public IList<ObjectId> Objects { get; set; }

        public BatchRequestMode Operation { get; set; }

        public IList<TransferMode> Transfers { get; set; }
    }
}
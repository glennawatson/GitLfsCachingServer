// --------------------------------------------------------------------------------------------------------------------
// <copyright file="Transfer.cs" company="Glenn Watson">
//     Copyright (C) 2017. Glenn Watson
// </copyright>
// --------------------------------------------------------------------------------------------------------------------

namespace GitLfs.Core.BatchResponse
{
    using System.Collections.Generic;

    public class Transfer
    {
        public TransferMode Mode { get; set; }

        public IList<BatchObject> Objects { get; set; }
    }
}
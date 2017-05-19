// --------------------------------------------------------------------------------------------------------------------
// <copyright file="Transfer.cs" company="Glenn Watson">
//     Copyright (C) 2017. Glenn Watson
// </copyright>
// --------------------------------------------------------------------------------------------------------------------

namespace GitLfs.Core.BatchResponse
{
    using System.Collections.Generic;

    /// <summary>
    /// Represents a transfer from the batch api in git LFS.
    /// </summary>
    public class BatchTransfer
    {
        /// <summary>
        /// The mode of the transfer.
        /// </summary>
        public TransferMode? Mode { get; set; }

        /// <summary>
        /// Gets the objects of the transfer.
        /// </summary>
        public IList<BatchObjectBase> Objects { get; set; }
    }
}
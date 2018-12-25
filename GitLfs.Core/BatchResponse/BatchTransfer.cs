// <copyright file="BatchTransfer.cs" company="Glenn Watson">
// Copyright (c) 2018 Glenn Watson. All rights reserved.
// See LICENSE file in the project root for full license information.
// </copyright>

namespace GitLfs.Core.BatchResponse
{
    using System.Collections.Generic;

    /// <summary>
    /// Represents a transfer from the batch api in git LFS.
    /// </summary>
    public class BatchTransfer
    {
        /// <summary>
        /// Gets or sets the mode of the transfer.
        /// </summary>
        public TransferMode? Mode { get; set; }

        /// <summary>
        /// Gets or sets the objects of the transfer.
        /// </summary>
        public IList<IBatchObject> Objects { get; set; }
    }
}
// <copyright file="BatchObjectAction.cs" company="Glenn Watson">
// Copyright (c) 2018 Glenn Watson. All rights reserved.
// See LICENSE file in the project root for full license information.
// </copyright>

namespace GitLfs.Core.BatchResponse
{
    using System;
    using System.Collections.Generic;
    using System.Diagnostics.CodeAnalysis;

    /// <summary>
    /// A batch request object and it's action inside LFS.
    /// </summary>
    public class BatchObjectAction
    {
        /// <summary>
        /// Gets or sets the time date in which the batch object expires as a ISO8601 string.
        /// </summary>
        public DateTime? ExpiresAt { get; set; }

        /// <summary>
        /// Gets or sets the time in whole seconds.
        /// </summary>
        public int? ExpiresIn { get; set; }

        /// <summary>
        /// Gets or sets the headers provided by the server.
        /// </summary>
        [SuppressMessage("Usage", "CA2227:Collection properties should be read only", Justification = "Used for serialisation.")]
        public List<BatchHeader> Headers { get; set; }

        /// <summary>
        /// Gets or sets the URL where to perform the upload or download operation.
        /// </summary>
        public string HRef { get; set; }

        /// <summary>
        /// Gets or sets the mode of the action.
        /// </summary>
        public BatchActionMode Mode { get; set; }
    }
}
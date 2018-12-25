// <copyright file="BatchObject.cs" company="Glenn Watson">
// Copyright (c) 2018 Glenn Watson. All rights reserved.
// See LICENSE file in the project root for full license information.
// </copyright>

namespace GitLfs.Core.BatchResponse
{
    using System.Collections.Generic;

    /// <summary>
    /// A object that we will perform batch operations on.
    /// </summary>
    public class BatchObject : IBatchObject
    {
        /// <summary>
        /// Gets or sets a collection of actions to perform for the object.
        /// </summary>
        public IList<BatchObjectAction> Actions { get; set; }

        /// <summary>
        /// Gets or sets a value indicating if we are authenticated.
        /// </summary>
        public bool? Authenticated { get; set; }

        /// <inheritdoc />
        public ObjectId Id { get; set; }
    }
}
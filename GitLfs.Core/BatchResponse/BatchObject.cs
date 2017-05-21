// --------------------------------------------------------------------------------------------------------------------
// <copyright file="BatchObject.cs" company="Glenn Watson">
//     Copyright (C) 2017. Glenn Watson
// </copyright>
// --------------------------------------------------------------------------------------------------------------------

namespace GitLfs.Core.BatchResponse
{
    using System;
    using System.Collections.Generic;
    using System.ComponentModel.DataAnnotations;
    using Newtonsoft.Json;

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
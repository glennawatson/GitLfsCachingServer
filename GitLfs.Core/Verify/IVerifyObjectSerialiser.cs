// <copyright file="IVerifyObjectSerialiser.cs" company="Glenn Watson">
// Copyright (c) 2018 Glenn Watson. All rights reserved.
// See LICENSE file in the project root for full license information.
// </copyright>

namespace GitLfs.Core.Verify
{
    using System;

    /// <summary>
    /// Serialises to and from a ObjectId.
    /// </summary>
    public interface IVerifyObjectSerialiser
    {
        /// <summary>
        /// Create a transfer object from a string.
        /// </summary>
        /// <param name="value">The string value to convert from.</param>
        /// <returns>The transfer object.</returns>
        ObjectId FromString(string value);

        /// <summary>
        /// Create a string value from a transfer.
        /// </summary>
        /// <param name="transfer">The transfer object to convert from.</param>
        /// <returns>The string value representing the transfer object.</returns>
        string ToString(ObjectId transfer);
    }
}

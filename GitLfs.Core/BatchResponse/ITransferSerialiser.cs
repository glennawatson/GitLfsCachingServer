// --------------------------------------------------------------------------------------------------------------------
// <copyright file="ITransferSerialiser.cs" company="Glenn Watson">
//   Copyright (C) 2017. Glenn Watson
// </copyright>
// <summary>
//   Allows serialisation of Transfer objects to and from string values.
// </summary>
// --------------------------------------------------------------------------------------------------------------------

namespace GitLfs.Core.BatchResponse
{
    /// <summary>
    /// Allows serialisation of Transfer objects to and from string values.
    /// </summary>
    public interface ITransferSerialiser
    {
        /// <summary>
        /// Create a transfer object from a string.
        /// </summary>
        /// <param name="value">The string value to convert from.</param>
        /// <returns>The transfer object.</returns>
        Transfer FromString(string value);

        /// <summary>
        /// Create a string value from a transfer.
        /// </summary>
        /// <param name="transfer">The transfer object to convert from.</param>
        /// <returns>The string value representing the transfer object.</returns>
        string ToString(Transfer transfer);
    }
}
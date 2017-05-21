// --------------------------------------------------------------------------------------------------------------------
// <copyright file="ITransferSerialiser.cs" company="Glenn Watson">
//     Copyright (C) 2017. Glenn Watson
// </copyright>
// --------------------------------------------------------------------------------------------------------------------

namespace GitLfs.Core.BatchResponse
{
    /// <summary>
    /// Allows serialisation of Transfer objects to and from string values.
    /// </summary>
    public interface IBatchTransferSerialiser
    {
        /// <summary>
        /// Create a transfer object from a string.
        /// </summary>
        /// <param name="value">The string value to convert from.</param>
        /// <returns>The transfer object.</returns>
        BatchTransfer TransferFromString(string value);

		/// <summary>
		/// Create a batch object from a string.
		/// </summary>
		/// <param name="value">The string value to convert from.</param>
		/// <returns>The batch object.</returns>
		BatchObject ObjectFromString(string value);

        /// <summary>
        /// Create a string value from a transfer.
        /// </summary>
        /// <param name="transfer">The transfer object to convert from.</param>
        /// <returns>The string value representing the transfer object.</returns>
        string ToString(BatchTransfer transfer);

        /// <summary>
        /// Converts a batch object into a string.
        /// </summary>
        /// <returns>The string.</returns>
        /// <param name="batchObject">Batch object.</param>
        string ToString(BatchObject batchObject);
    }
}
// <copyright file="IBatchRequestSerialiser.cs" company="Glenn Watson">
//    Copyright (C) 2017. Glenn Watson
// </copyright>

namespace GitLfs.Core.BatchRequest
{
    /// <summary>
    /// Serialises a request object to and from a string.
    /// </summary>
    public interface IBatchRequestSerialiser
    {
        /// <summary>
        /// Converts a string into a Request object.
        /// </summary>
        /// <param name="value">The value to convert.</param>
        /// <returns>A request object.</returns>
        BatchRequest FromString(string value);

        /// <summary>
        /// Converts a request object into a string.
        /// </summary>
        /// <param name="value">The request object to convert.</param>
        /// <returns>The string representation of the request.</returns>
        string ToString(BatchRequest value);
    }
}
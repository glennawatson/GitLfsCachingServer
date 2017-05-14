// --------------------------------------------------------------------------------------------------------------------
// <copyright file="IRequestSerialiser.cs" company="Glenn Watson">
//   Copyright (C) 2017. Glenn Watson
// </copyright>
// <summary>
//   Serialises a request object to and from a string.
// </summary>
// --------------------------------------------------------------------------------------------------------------------

namespace GitLfs.Core.BatchRequest
{
    /// <summary>
    ///     Serialises a request object to and from a string.
    /// </summary>
    public interface IRequestSerialiser
    {
        /// <summary>
        ///     Converts a string into a Request object.
        /// </summary>
        /// <param name="value">The value to convert.</param>
        /// <returns>A request object.</returns>
        Request FromString(string value);

        /// <summary>
        ///     Converts a request object into a string.
        /// </summary>
        /// <param name="value">The request object to convert.</param>
        /// <returns>The string representation of the request.</returns>
        string ToString(Request value);
    }
}
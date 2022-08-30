// <copyright file="LfsException.cs" company="Glenn Watson">
// Copyright (c) 2018 Glenn Watson. All rights reserved.
// See LICENSE file in the project root for full license information.
// </copyright>

namespace GitLfs.Core;

using System;

/// <summary>
/// A exception that occurs if there is an error caused by the LFS system.
/// </summary>
public class LfsException : Exception
{
    /// <summary>
    /// Initializes a new instance of the <see cref="LfsException"/> class.
    /// </summary>
    public LfsException()
    {
    }

    /// <summary>
    /// Initializes a new instance of the <see cref="LfsException"/> class.
    /// </summary>
    /// <param name="message">The message about the exception.</param>
    public LfsException(string message)
        : base(message)
    {
    }

    /// <summary>
    /// Initializes a new instance of the <see cref="LfsException" /> class.
    /// It will contain the specified error message and a reference to the inner exception that is the cause of this exception.
    /// </summary>
    /// <param name="message">The error message that explains the reason for the exception.</param>
    /// <param name="innerException">The exception that is the cause of the current exception, or a null reference (Nothing in Visual Basic) if no inner exception is specified.</param>
    public LfsException(string message, Exception innerException)
        : base(message, innerException)
    {
    }
}

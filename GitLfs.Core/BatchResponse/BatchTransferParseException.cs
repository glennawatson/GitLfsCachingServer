// --------------------------------------------------------------------------------------------------------------------
// <copyright file="TransferParseException.cs" company="Glenn Watson">
//     Copyright (C) 2017. Glenn Watson
// </copyright>
// --------------------------------------------------------------------------------------------------------------------

namespace GitLfs.Core.BatchResponse
{
    using System;

    public class BatchTransferParseException : Exception
    {
        public BatchTransferParseException()
        {
        }

        public BatchTransferParseException(string message)
            : base(message)
        {
        }

        public BatchTransferParseException(string message, Exception innerException)
            : base(message, innerException)
        {
        }
    }
}
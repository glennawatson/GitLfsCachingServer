namespace GitLfs.Core.BatchResponse
{
    using System;

    public class TransferParseException : Exception
    {
        public TransferParseException()
        {
        }

        public TransferParseException(string message)
            : base(message)
        {
        }

        public TransferParseException(string message, Exception innerException)
            : base(message, innerException)
        {
        }
    }
}
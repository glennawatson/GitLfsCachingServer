namespace GitLfs.Client
{
    using System;
    using System.Net;

    public class ClientDownloadException : Exception
    {
        public ClientDownloadException(HttpStatusCode statusCode, string message)
            : base(message)
        {
            this.StatusCode = (int)statusCode;
        }

        public ClientDownloadException(int statusCode, string message)
            : base(message)
        {
            this.StatusCode = statusCode;
        }

        public int StatusCode { get; }
    }
}

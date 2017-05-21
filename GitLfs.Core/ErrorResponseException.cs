namespace GitLfs.Core
{
	using GitLfs.Core.Error;

    /// <summary>
    /// An exception that includes a error response.
    /// </summary>
	public class ErrorResponseException : StatusCodeException
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="T:GitLfs.Core.ErrorResponseException"/> class.
        /// </summary>
        /// <param name="response">The response.</param>
        public ErrorResponseException(ErrorResponse response, int? statusCode = null)
            : base(statusCode, response.Message)
        {
            this.ErrorResponse = response;
        }

        /// <summary>
        /// Gets the error response.
        /// </summary>
        public ErrorResponse ErrorResponse { get; }
    }
}

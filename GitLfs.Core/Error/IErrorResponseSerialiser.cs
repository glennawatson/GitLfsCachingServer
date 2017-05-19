namespace GitLfs.Core.Error
{
    public interface IErrorResponseSerialiser
    {
        /// <summary>
        /// Create a transfer object from a string.
        /// </summary>
        /// <param name="value">The string value to convert from.</param>
        /// <returns>The transfer object.</returns>
        ErrorResponse FromString(string value);

        /// <summary>
        /// Create a string value from a transfer.
        /// </summary>
        /// <param name="transfer">The transfer object to convert from.</param>
        /// <returns>The string value representing the transfer object.</returns>
        string ToString(ErrorResponse transfer);
    }
}

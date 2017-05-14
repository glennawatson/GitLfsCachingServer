// --------------------------------------------------------------------------------------------------------------------
// <copyright file="ErrorResponse.cs" company="Glenn Watson">
//     Copyright (C) 2017. Glenn Watson
// </copyright>
// --------------------------------------------------------------------------------------------------------------------

namespace GitLfs.Core
{
    using Newtonsoft.Json;

    /// <summary>
    /// Indicates a error response to the user.
    /// </summary>
    public class ErrorResponse
    {
        /// <summary>
        /// Gets or sets a URL to show to the end user.
        /// </summary>
        [JsonProperty(PropertyName = "documentation_url")]
        public string DocumentationUrl { get; set; }

        /// <summary>
        /// Gets or sets a user friendly string to show to the user.
        /// </summary>
        [JsonProperty(PropertyName = "message")]
        public string Message { get; set; }

        /// <summary>
        /// Gets or sets a UID to associate with the request.
        /// </summary>
        [JsonProperty(PropertyName = "request_id")]
        public string RequestId { get; set; }
    }
}
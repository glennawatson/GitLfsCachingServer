// <copyright file="ErrorResponse.cs" company="Glenn Watson">
// Copyright (c) 2018 Glenn Watson. All rights reserved.
// See LICENSE file in the project root for full license information.
// </copyright>

namespace GitLfs.Core.ErrorHandling
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

        /// <inheritdoc />
        public override string ToString()
        {
            return
                $"[ErrorResponse: DocumentationUrl={this.DocumentationUrl}, Message={this.Message}, RequestId={this.RequestId}]";
        }
    }
}
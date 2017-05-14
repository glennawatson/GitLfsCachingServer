namespace GitLfs.Core
{
    using Newtonsoft.Json;

    public class ErrorResponse
    {
        [JsonProperty(PropertyName = "message")]
        public string Message { get; set; }

        [JsonProperty(PropertyName = "documentation_url")]
        public string DocumentationUrl { get; set; }

        [JsonProperty(PropertyName = "request_id")]
        public string RequestId { get; set; }
    }
}

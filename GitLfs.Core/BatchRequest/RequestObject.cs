namespace GitLfs.Core.BatchRequest
{
    using Newtonsoft.Json;

    public class RequestObject
    {
        [JsonProperty(PropertyName = "oid")]
        public string ObjectId { get; set; }

        public long Size { get; set; }
    }
}

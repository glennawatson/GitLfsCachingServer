namespace GitLfs.Core.BatchResponse
{
    using Newtonsoft.Json;

    public abstract class BatchObjectBase
    {
        [JsonProperty(PropertyName = "oid")]
        public string ObjectId { get; set; }

        public long Size { get; set; }
    }
}

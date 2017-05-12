namespace GitLfs.Core
{
    using System.Collections;
    using System.Collections.Generic;

    using Newtonsoft.Json;

    public class BatchObject
    {
        [JsonProperty(PropertyName = "oid")]
        public string ObjectId { get; set; }

        public long Size { get; set; }

        public bool? Authenticated { get; set; }

        public IList<BatchObjectAction> Actions { get; set; }
    }
}

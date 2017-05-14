namespace GitLfs.Core.BatchResponse
{
    using System;
    using System.Collections.Generic;

    public class BatchObjectAction
    {
        public DateTime? ExpiresAt { get; set; }

        public int? ExpiresIn { get; set; }

        public IList<KeyValuePair<string, string>> Headers { get; set; }

        public string HRef { get; set; }

        public BatchActionMode Mode { get; set; }
    }
}
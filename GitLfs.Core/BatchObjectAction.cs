namespace GitLfs.Core
{
    using System;
    using System.Collections.Generic;

    public class BatchObjectAction
    {
        public IList<KeyValuePair<string, string>> Header { get; set; }

        public string HRef { get; set; }

        public BatchActionMode Mode { get; set; }

        public int? ExpiresIn { get; set; }

        public DateTime? ExpiresAt { get; set; } 
    }
}

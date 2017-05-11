namespace GitLfs.Core
{
    using System.Collections;
    using System.Collections.Generic;

    public class BatchObjectDescriptor
    {
        public IList<KeyValuePair<string, string>> Header { get; set; }

        public string HRef { get; set; }

        public BatchAction Action { get; set; }
    }
}

namespace GitLfs.Core.BatchResponse
{
    using System.Collections.Generic;

    public class Transfer
    {
        public TransferMode Mode { get; set; }

        public IList<BatchObject> Objects { get; set; }
    }
}

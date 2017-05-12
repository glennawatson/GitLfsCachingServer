namespace GitLfs.Core
{
    using System.Collections;
    using System.Collections.Generic;

    public class Transfer
    {
        public TransferMode Mode { get; set; }

        public IList<BatchObject> Objects { get; set; }
    }
}

namespace GitLfs.Core.BatchRequest
{
    using System.Collections;
    using System.Collections.Generic;

    public class Request
    {
        public RequestMode Operation { get; set; }

        public IList<TransferMode> Transfers { get; set; }

        public IList<RequestObject> Objects { get; set; }
    }
}

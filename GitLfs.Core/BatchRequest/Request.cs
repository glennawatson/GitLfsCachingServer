namespace GitLfs.Core.BatchRequest
{
    using System.Collections.Generic;

    public class Request
    {
        public IList<RequestObject> Objects { get; set; }

        public RequestMode Operation { get; set; }

        public IList<TransferMode> Transfers { get; set; }
    }
}
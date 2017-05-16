namespace GitLfs.Core.BatchResponse
{
    public class BatchObjectError : BatchObjectBase
    {
        public int ErrorCode { get; set; }

        public string ErrorMessage { get; set; }
    }
}

namespace GitLfs.Core.BatchResponse
{
    public class BatchObjectError : IBatchObject
    {
		/// <inheritdoc />
		public ObjectId Id { get; set; }

		public int ErrorCode { get; set; }

        public string ErrorMessage { get; set; }
    }
}

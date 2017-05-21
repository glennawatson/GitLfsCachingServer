namespace GitLfs.Core.BatchResponse
{
    using System.ComponentModel.DataAnnotations;

	public class BatchHeader
    {
        public BatchHeader()
        {
        }

        public BatchHeader(string key, string value)
        {
            this.Key = key;
            this.Value = value;
        }

        public string Key { get; set; }

        public string Value { get; set; }
    }
}

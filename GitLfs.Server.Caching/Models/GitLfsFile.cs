namespace GitLfs.Server.Caching.Models
{
    public class GitLfsFile
    {
        public int Id { get; set; }
        public string ObjectId { get; set; }
        public long Size { get; set; }
    }
}

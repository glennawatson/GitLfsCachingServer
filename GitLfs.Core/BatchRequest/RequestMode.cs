namespace GitLfs.Core.BatchRequest
{
    using System.Runtime.Serialization;

    public enum RequestMode
    {
        [EnumMember(Value = "upload")]
        Upload,

        [EnumMember(Value = "download")]
        Download
    }
}

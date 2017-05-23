// <copyright file="BatchHeader.cs" company="Glenn Watson">
//    Copyright (C) 2017. Glenn Watson
// </copyright>

namespace GitLfs.Core.BatchResponse
{
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
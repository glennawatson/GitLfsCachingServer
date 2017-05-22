// <copyright file="BatchObjectError.cs" company="Glenn Watson">
//    Copyright (C) 2017. Glenn Watson
// </copyright>

namespace GitLfs.Core.BatchResponse
{
    public class BatchObjectError : IBatchObject
    {
        public int ErrorCode { get; set; }

        public string ErrorMessage { get; set; }

        /// <inheritdoc />
        public ObjectId Id { get; set; }
    }
}
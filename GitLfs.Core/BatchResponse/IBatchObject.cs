// <copyright file="IBatchObject.cs" company="Glenn Watson">
//    Copyright (C) 2017. Glenn Watson
// </copyright>

namespace GitLfs.Core.BatchResponse
{
    public interface IBatchObject
    {
        ObjectId Id { get; set; }
    }
}
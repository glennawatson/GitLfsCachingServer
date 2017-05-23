// <copyright file="LfsException.cs" company="Glenn Watson">
//    Copyright (C) 2017. Glenn Watson
// </copyright>

namespace GitLfs.Core
{
    using System;

    public class LfsException : Exception
    {
        public LfsException()
        {
        }

        public LfsException(string message)
            : base(message)
        {
        }

        public LfsException(string message, Exception inner)
            : base(message, inner)
        {
        }
    }
}
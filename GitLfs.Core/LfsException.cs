﻿namespace GitLfs.Core
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

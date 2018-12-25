// <copyright file="CachingStream.cs" company="Glenn Watson">
// Copyright (c) 2018 Glenn Watson. All rights reserved.
// See LICENSE file in the project root for full license information.
// </copyright>

namespace GitLfs.Core.File
{
    using System;
    using System.IO;

    /// <summary>
    /// Writes to an output file as an input stream is read.
    /// </summary>
    internal class CachingStream : Stream
    {
        private readonly Stream inStream;
        private readonly string path;
        private FileStream outStream;

        /// <summary>
        /// Initializes a new instance of the <see cref="CachingStream"/> class.
        /// </summary>
        /// <param name="inStream">The input stream we will use to cache against.</param>
        /// <param name="path">The path to where the are going to write to.</param>
        public CachingStream(Stream inStream, string path)
        {
            this.inStream = inStream;
            this.path = path;
            this.outStream = new FileStream(path, FileMode.Create, FileAccess.Write);
        }

        /// <inheritdoc />
        public override bool CanRead => true;

        /// <inheritdoc />
        public override bool CanSeek => false;

        /// <inheritdoc />
        public override bool CanWrite => false;

        /// <inheritdoc />
        public override long Length => throw new NotSupportedException();

        /// <inheritdoc />
        public override long Position
        {
            get => throw new NotSupportedException();
            set => throw new NotSupportedException();
        }

        /// <inheritdoc />
        public override void Flush()
        {
        }

        /// <inheritdoc />
        public override long Seek(long offset, SeekOrigin origin)
        {
            throw new NotSupportedException();
        }

        /// <inheritdoc />
        public override void SetLength(long value)
        {
            throw new NotSupportedException();
        }

        /// <inheritdoc />
        public override int Read(byte[] buffer, int offset, int count)
        {
            try
            {
                int bytesRead = this.inStream.Read(buffer, offset, count);
                if (bytesRead != 0)
                {
                    this.outStream.Write(buffer, offset, bytesRead);
                }

                return bytesRead;
            }
            catch (Exception)
            {
                this.outStream.Dispose();
                this.outStream = null;
                System.IO.File.Delete(this.path);
                throw;
            }
        }

        /// <inheritdoc />
        public override void Write(byte[] buffer, int offset, int count)
        {
            throw new NotSupportedException();
        }

        /// <inheritdoc />
        protected override void Dispose(bool disposing)
        {
            if (this.outStream != null)
            {
                this.outStream.Dispose();
                this.outStream = null;
            }

            base.Dispose(disposing);
        }
    }
}

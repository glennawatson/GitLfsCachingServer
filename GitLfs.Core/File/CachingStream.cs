// <copyright file="CachingStream.cs" company="Glenn Watson">
// Copyright (c) 2018 Glenn Watson. All rights reserved.
// See LICENSE file in the project root for full license information.
// </copyright>

namespace GitLfs.Core.File;

using System;
using System.IO;

/// <summary>
/// Writes to an output file as an input stream is read.
/// </summary>
internal class CachingStream : Stream
{
    private readonly Stream _inStream;
    private readonly string _path;
    private FileStream _outStream;

    /// <summary>
    /// Initializes a new instance of the <see cref="CachingStream"/> class.
    /// </summary>
    /// <param name="inStream">The input stream we will use to cache against.</param>
    /// <param name="path">The path to where the are going to write to.</param>
    public CachingStream(Stream inStream, string path)
    {
        _inStream = inStream;
        _path = path;
        _outStream = new FileStream(path, FileMode.Create, FileAccess.Write);
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
            int bytesRead = _inStream.Read(buffer, offset, count);
            if (bytesRead != 0)
            {
                _outStream.Write(buffer, offset, bytesRead);
            }

            return bytesRead;
        }
        catch (Exception)
        {
            _outStream.Dispose();
            _outStream = null;
            System.IO.File.Delete(_path);
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
        if (_outStream != null)
        {
            _outStream.Dispose();
            _outStream = null;
        }

        base.Dispose(disposing);
    }
}

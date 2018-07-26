using System;
using System.IO;

namespace GitLfs.Core.File
{
	/// <summary>
	/// Writes to an output file as an input stream is read
	/// </summary>
	internal class CachingStream : Stream
	{
		private readonly Stream _inStream;
		private readonly string _path;
		private FileStream _outStream;

		public CachingStream(Stream inStream, string path)
		{
			_inStream = inStream;
			_path = path;
			_outStream = new FileStream(path, FileMode.Create, FileAccess.Write);
		}

		public override void Flush()
		{
		}

		public override long Seek(long offset, SeekOrigin origin)
		{
			throw new NotSupportedException();
		}

		public override void SetLength(long value)
		{
			throw new NotSupportedException();
		}

		public override int Read(byte[] buffer, int offset, int count)
		{
			try
			{
				int bytesRead = _inStream.Read(buffer, offset, count);
				if (bytesRead != 0)
					_outStream.Write(buffer, offset, bytesRead);

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

		public override void Write(byte[] buffer, int offset, int count)
		{
			throw new NotSupportedException();
		}

		public override bool CanRead => true;
		public override bool CanSeek => false;
		public override bool CanWrite => false;

		public override long Length
		{
			get { throw new NotSupportedException(); }
		}

		public override long Position
		{
			get { throw new NotSupportedException(); }
			set { throw new NotSupportedException(); }
		}

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
}

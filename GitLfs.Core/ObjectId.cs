// <copyright file="ObjectId.cs" company="Glenn Watson">
//    Copyright (C) 2017. Glenn Watson
// </copyright>

namespace GitLfs.Core
{
    using Newtonsoft.Json;

    /// <summary>
    /// Represents a LFS object id.
    /// </summary>
    public class ObjectId
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="T:GitLfs.Core.ObjectId" /> class.
        /// </summary>
        public ObjectId()
        {
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="T:GitLfs.Core.ObjectId" /> class.
        /// </summary>
        /// <param name="hash">The SHA256 hash of the object.</param>
        /// <param name="size">The size of the object.</param>
        public ObjectId(string hash, long size)
        {
            this.Hash = hash;
            this.Size = size;
        }

        /// <summary>
        /// Gets the SHA256 hash of the object.
        /// </summary>
        [JsonProperty(PropertyName = "oid")]
        public string Hash { get; set; }

        /// <summary>
        /// Gets the size of the object.
        /// </summary>
        [JsonProperty(PropertyName = "size")]
        public long Size { get; set; }

        /// <inheritdoc />
        public override bool Equals(object obj)
        {
            if (obj == null || !(obj is ObjectId))
            {
                return false;
            }

            var otherObjectId = (ObjectId)obj;

            if (Equals(otherObjectId.Hash, this.Hash) == false)
            {
                return false;
            }

            if (otherObjectId.Size != this.Size)
            {
                return false;
            }

            return true;
        }

        /// <inheritdoc />
        public override int GetHashCode()
        {
            return this.Hash.GetHashCode() ^ this.Size.GetHashCode();
        }

        /// <inheritdoc />
        public override string ToString()
        {
            return $"SHA256: {this.Hash}, Size: {this.Size}";
        }
    }
}
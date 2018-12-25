// <copyright file="ObjectId.cs" company="Glenn Watson">
// Copyright (c) 2018 Glenn Watson. All rights reserved.
// See LICENSE file in the project root for full license information.
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
        /// Initializes a new instance of the <see cref="ObjectId"/> class.
        /// </summary>
        public ObjectId()
        {
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="ObjectId"/> class.
        /// </summary>
        /// <param name="hash">The SHA256 hash of the object.</param>
        /// <param name="size">The size of the object.</param>
        public ObjectId(string hash, long size)
        {
            this.Hash = hash;
            this.Size = size;
        }

        /// <summary>
        /// Gets or sets the SHA256 hash of the object.
        /// </summary>
        [JsonProperty(PropertyName = "oid")]
        public string Hash { get; set; }

        /// <summary>
        /// Gets or sets the size of the object.
        /// </summary>
        [JsonProperty(PropertyName = "size")]
        public long Size { get; set; }

        /// <inheritdoc />
        public override bool Equals(object obj)
        {
            if (!(obj is ObjectId))
            {
                return false;
            }

            var otherObjectId = (ObjectId)obj;

            if (!Equals(otherObjectId.Hash, this.Hash))
            {
                return false;
            }

            return otherObjectId.Size == this.Size;
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
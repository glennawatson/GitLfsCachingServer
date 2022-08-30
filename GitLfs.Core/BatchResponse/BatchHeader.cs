// <copyright file="BatchHeader.cs" company="Glenn Watson">
// Copyright (c) 2018 Glenn Watson. All rights reserved.
// See LICENSE file in the project root for full license information.
// </copyright>

namespace GitLfs.Core.BatchResponse;

/// <summary>
/// A header for a batch request.
/// </summary>
public class BatchHeader
{
    /// <summary>
    /// Initializes a new instance of the <see cref="BatchHeader"/> class.
    /// </summary>
    public BatchHeader()
    {
    }

    /// <summary>
    /// Initializes a new instance of the <see cref="BatchHeader"/> class.
    /// </summary>
    /// <param name="key">The key of the batch request header item.</param>
    /// <param name="value">The value of the batch request header item.</param>
    public BatchHeader(string key, string value)
    {
        Key = key;
        Value = value;
    }

    /// <summary>
    /// Gets or sets the key of the batch request header item.
    /// </summary>
    public string Key { get; set; }

    /// <summary>
    /// Gets or sets the value of the batch request header item.
    /// </summary>
    public string Value { get; set; }
}

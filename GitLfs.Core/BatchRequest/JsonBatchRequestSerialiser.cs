// <copyright file="JsonBatchRequestSerialiser.cs" company="Glenn Watson">
// Copyright (c) 2018 Glenn Watson. All rights reserved.
// See LICENSE file in the project root for full license information.
// </copyright>

namespace GitLfs.Core.BatchRequest;

using System.Diagnostics.CodeAnalysis;
using Newtonsoft.Json;
using Newtonsoft.Json.Converters;
using Newtonsoft.Json.Serialization;

/// <summary>
/// Serialises request objects to and from JSON.
/// </summary>
public class JsonBatchRequestSerialiser : IBatchRequestSerialiser
{
    /// <inheritdoc />
    public BatchRequest FromString(string value)
    {
        try
        {
            return JsonConvert.DeserializeObject<BatchRequest>(value, CreateSettings());
        }
        catch (JsonException ex)
        {
            throw new ParseException(ex.Message, ex);
        }
    }

    /// <inheritdoc />
    public string ToString(BatchRequest value)
    {
        try
        {
            return JsonConvert.SerializeObject(value, CreateSettings());
        }
        catch (JsonException ex)
        {
            throw new ParseException(ex.Message, ex);
        }
    }

    private static JsonSerializerSettings CreateSettings()
    {
        var settings = new JsonSerializerSettings
        {
            Formatting = Formatting.Indented,
        };
        settings.Converters.Add(new StringEnumConverter());
        settings.ContractResolver = new LowercaseContractResolver();
        return settings;
    }

    private class LowercaseContractResolver : DefaultContractResolver
    {
        [SuppressMessage("Globalization", "CA1308:Normalize strings to uppercase", Justification = "Needed by LFS standard.")]
        protected override string ResolvePropertyName(string propertyName)
        {
            return propertyName.ToLowerInvariant();
        }
    }
}

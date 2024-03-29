﻿// <copyright file="JsonVerifyObjectSerialiser.cs" company="Glenn Watson">
// Copyright (c) 2018 Glenn Watson. All rights reserved.
// See LICENSE file in the project root for full license information.
// </copyright>

namespace GitLfs.Core.Verify;

using Newtonsoft.Json;
using Newtonsoft.Json.Converters;

/// <summary>
/// Serialises a verify object.
/// </summary>
public class JsonVerifyObjectSerialiser : IVerifyObjectSerialiser
{
    /// <inheritdoc />
    public ObjectId FromString(string value)
    {
        try
        {
            return JsonConvert.DeserializeObject<ObjectId>(value, CreateSettings());
        }
        catch (JsonException ex)
        {
            throw new ParseException(ex.Message, ex);
        }
    }

    /// <inheritdoc />
    public string ToString(ObjectId transfer)
    {
        try
        {
            return JsonConvert.SerializeObject(transfer, CreateSettings());
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

        return settings;
    }
}

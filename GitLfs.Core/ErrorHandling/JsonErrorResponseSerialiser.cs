// <copyright file="JsonErrorResponseSerialiser.cs" company="Glenn Watson">
// Copyright (c) 2018 Glenn Watson. All rights reserved.
// See LICENSE file in the project root for full license information.
// </copyright>

namespace GitLfs.Core.ErrorHandling
{
    using Newtonsoft.Json;
    using Newtonsoft.Json.Converters;

    /// <summary>
    /// Serialiser that will convert error responses into java script.
    /// </summary>
    public class JsonErrorResponseSerialiser : IErrorResponseSerialiser
    {
        /// <inheritdoc />
        public ErrorResponse FromString(string value)
        {
            try
            {
                return JsonConvert.DeserializeObject<ErrorResponse>(value, CreateSettings());
            }
            catch (JsonException ex)
            {
                throw new ParseException(ex.Message, ex);
            }
        }

        /// <inheritdoc />
        public string ToString(ErrorResponse transfer)
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
                Formatting = Formatting.Indented
            };
            settings.Converters.Add(new StringEnumConverter());
            return settings;
        }
    }
}
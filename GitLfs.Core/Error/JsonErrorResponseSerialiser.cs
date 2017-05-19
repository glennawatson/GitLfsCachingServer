// --------------------------------------------------------------------------------------------------------------------
// <copyright file="JsonErrorResponseSerialiser.cs" company="Glenn Watson">
//     Copyright (C) 2017. Glenn Watson
// </copyright>
// --------------------------------------------------------------------------------------------------------------------

namespace GitLfs.Core.Error
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
            return JsonConvert.DeserializeObject<ErrorResponse>(value, CreateSettings());
        }

        /// <inheritdoc />
        public string ToString(ErrorResponse value)
        {
            return JsonConvert.SerializeObject(value, CreateSettings());
        }

        private static JsonSerializerSettings CreateSettings()
        {
            var settings = new JsonSerializerSettings();
            settings.Formatting = Formatting.Indented;
            settings.Converters.Add(new StringEnumConverter());
            return settings;
        }
    }
}
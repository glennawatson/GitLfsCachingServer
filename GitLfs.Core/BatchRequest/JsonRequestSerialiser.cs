// --------------------------------------------------------------------------------------------------------------------
// <copyright file="JsonRequestSerialiser.cs" company="Glenn Watson">
//     Copyright (C) 2017. Glenn Watson
// </copyright>
// --------------------------------------------------------------------------------------------------------------------

namespace GitLfs.Core.BatchRequest
{
    using Newtonsoft.Json;
    using Newtonsoft.Json.Converters;
    using Newtonsoft.Json.Serialization;

    /// <summary>
    /// Serialises request objects to and from JSON.
    /// </summary>
    public class JsonRequestSerialiser : IRequestSerialiser
    {
        /// <inheritdoc />
        public Request FromString(string value)
        {
            return JsonConvert.DeserializeObject<Request>(value, CreateSettings());
        }

        /// <inheritdoc />
        public string ToString(Request value)
        {
            return JsonConvert.SerializeObject(value, CreateSettings());
        }

        private static JsonSerializerSettings CreateSettings()
        {
            var settings = new JsonSerializerSettings();
            settings.Formatting = Formatting.Indented;
            settings.Converters.Add(new StringEnumConverter());
            settings.ContractResolver = new LowercaseContractResolver();
            return settings;
        }

        private class LowercaseContractResolver : DefaultContractResolver
        {
            protected override string ResolvePropertyName(string propertyName)
            {
                return propertyName.ToLower();
            }
        }
    }
}
// --------------------------------------------------------------------------------------------------------------------
// <copyright file="TransferSerializerTests.cs" company="Glenn Watson">
//     Copyright (C) 2017. Glenn Watson
// </copyright>
// --------------------------------------------------------------------------------------------------------------------

namespace GitLfs.Core.Tests
{
    using GitLfs.Core.BatchResponse;

    using Newtonsoft.Json;
    using Newtonsoft.Json.Linq;

    using Xunit;

    public class TransferSerializerTests
    {
        [Fact]
        public void DeserialiseTest()
        {
            var testData =
                "{  \"transfer\": \"basic\",  \"objects\": [    {      \"oid\": \"1111111\",      \"size\": 123,      \"authenticated\": true,      \"actions\": {        \"download\": {          \"href\": \"https://some-download.com\",          \"header\": {            \"Key\": \"value\"          },          \"expires_at\": \"2016-11-10T15:29:07Z\",        }      }    }  ]}";

            JObject jsonObject = JObject.Parse(testData);

            var serialiser = new JsonBatchTransferSerialiser();
            BatchTransfer transfer = serialiser.TransferFromString(testData);

            string jsonText = serialiser.ToString(transfer);

            string formattedText = jsonObject.ToString(Formatting.Indented);

            Assert.Equal(formattedText, jsonText);
        }

        [Fact]
        public void SerialiseTest()
        {
            var testData =
                "{  \"transfer\": \"basic\",  \"objects\": [    {      \"oid\": \"1111111\",      \"size\": 123,      \"authenticated\": true,      \"actions\": {        \"download\": {          \"href\": \"https://some-download.com\",          \"header\": {            \"Key\": \"value\"          },          \"expires_at\": \"2016-11-10T15:29:07Z\",        }      }    }  ]}";

            var serialiser = new JsonBatchTransferSerialiser();
            BatchTransfer transfer = serialiser.TransferFromString(testData);

            Assert.Equal(transfer.Mode, TransferMode.Basic);

            Assert.Equal(transfer.Objects.Count, 1);
        }
    }
}
namespace GitLfs.Core.Tests
{
    using System;

    using GitLfs.Core.BatchResponse;

    using Newtonsoft.Json;
    using Newtonsoft.Json.Linq;

    using Xunit;

    public class TransferSerializerTests
    {
        [Fact]
        public void SerialiseTest()
        {
            var testData =
                "{  \"transfer\": \"basic\",  \"objects\": [    {      \"oid\": \"1111111\",      \"size\": 123,      \"authenticated\": true,      \"actions\": {        \"download\": {          \"href\": \"https://some-download.com\",          \"header\": {            \"Key\": \"value\"          },          \"expires_at\": \"2016-11-10T15:29:07Z\",        }      }    }  ]}";

            var transfer = JsonTransferSerialiser.FromJson(testData);

            Assert.Equal(transfer.Mode, TransferMode.Basic);

            Assert.Equal(transfer.Objects.Count, 1);
        }

        [Fact]
        public void DeserialiseTest()
        {
            var testData =
                "{  \"transfer\": \"basic\",  \"objects\": [    {      \"oid\": \"1111111\",      \"size\": 123,      \"authenticated\": true,      \"actions\": {        \"download\": {          \"href\": \"https://some-download.com\",          \"header\": {            \"Key\": \"value\"          },          \"expires_at\": \"2016-11-10T15:29:07Z\",        }      }    }  ]}";

            JObject jsonObject = JObject.Parse(testData);

            var transfer = JsonTransferSerialiser.FromJson(testData);

            var jsonText = JsonTransferSerialiser.ToJson(transfer);

            var formattedText = jsonObject.ToString(Formatting.Indented);

            Assert.Equal(formattedText, jsonText);
        }
    }
}

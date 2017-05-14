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

            var jsonObject = JObject.Parse(testData);

            var serialiser = new JsonTransferSerialiser();
            var transfer = serialiser.FromString(testData);

            var jsonText = serialiser.ToString(transfer);

            var formattedText = jsonObject.ToString(Formatting.Indented);

            Assert.Equal(formattedText, jsonText);
        }

        [Fact]
        public void SerialiseTest()
        {
            var testData =
                "{  \"transfer\": \"basic\",  \"objects\": [    {      \"oid\": \"1111111\",      \"size\": 123,      \"authenticated\": true,      \"actions\": {        \"download\": {          \"href\": \"https://some-download.com\",          \"header\": {            \"Key\": \"value\"          },          \"expires_at\": \"2016-11-10T15:29:07Z\",        }      }    }  ]}";

            var serialiser = new JsonTransferSerialiser();
            var transfer = serialiser.FromString(testData);

            Assert.Equal(transfer.Mode, TransferMode.Basic);

            Assert.Equal(transfer.Objects.Count, 1);
        }
    }
}
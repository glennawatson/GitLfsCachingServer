namespace GitLfs.Core.Tests
{
    using System;
    using System.Linq;
    using Xunit;

    public class TransferSerialiserTests
    {
        [Fact]
        public void SerialiseTest()
        {
            var testData =
                "{  \"transfer\": \"basic\",  \"objects\": [    {      \"oid\": \"1111111\",      \"size\": 123,      \"authenticated\": true,      \"actions\": {        \"download\": {          \"href\": \"https://some-download.com\",          \"header\": {            \"Key\": \"value\"          },          \"expires_at\": \"2016-11-10T15:29:07Z\",        }      }    }  ]}";

            var transfer = TransferSerialiser.SerialiseFromJson(testData);

            Assert.Equal(transfer.Mode, TransferMode.Basic);

            Assert.Equal(transfer.Objects.Count, 1);
        }

        [Fact]
        public void DeserialiseTest()
        {
            var testData =
                "{  \"transfer\": \"basic\",  \"objects\": [    {      \"oid\": \"1111111\",      \"size\": 123,      \"authenticated\": true,      \"actions\": {        \"download\": {          \"href\": \"https://some-download.com\",          \"header\": {            \"Key\": \"value\"          },          \"expires_at\": \"2016-11-10T15:29:07Z\",        }      }    }  ]}";

            var transfer = TransferSerialiser.SerialiseFromJson(testData);

            var jsonText = TransferSerialiser.SerialiseToJson(transfer);

            Assert.Equal(testData, jsonText);
        }
    }
}

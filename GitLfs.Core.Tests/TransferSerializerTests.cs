// <copyright file="TransferSerializerTests.cs" company="Glenn Watson">
//    Copyright (C) 2017. Glenn Watson
// </copyright>

namespace GitLfs.Core.Tests
{
    using GitLfs.Core.BatchResponse;

    using Newtonsoft.Json;
    using Newtonsoft.Json.Linq;
    using System;
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
        public void DeserialiseObjectActionTest()
        {

            var testData = "{\"download\": {  \"href\": \"https://github-cloud.s3.amazonaws.com/alambic/media/148182185/0a/f2/0af22f9dadcc067af162661e1b302481eefef87b36d29b75499d0f788092d807?X-Amz-Algorithm=AWS4-HMAC-SHA256&X-Amz-Credential=AKIAIMWPLRQEC4XCWWPA%2F20170522%2Fus-east-1%2Fs3%2Faws4_request&X-Amz-Date=20170522T051527Z&X-Amz-Expires=86400&X-Amz-Signature=f6c916650fe11c8ec33ed73c727a9dcfd71beac8569bc3819ca4634055ba8a38&X-Amz-SignedHeaders=host&actor_id=28613580&token=1\",  \"expires_in\": 3600,  \"expires_at\": \"2017-05-23T05:15:27Z\"}}";

            var action = new BatchObjectAction()
            {
                HRef = "https://github-cloud.s3.amazonaws.com/alambic/media/148182185/0a/f2/0af22f9dadcc067af162661e1b302481eefef87b36d29b75499d0f788092d807?X-Amz-Algorithm=AWS4-HMAC-SHA256&X-Amz-Credential=AKIAIMWPLRQEC4XCWWPA%2F20170522%2Fus-east-1%2Fs3%2Faws4_request&X-Amz-Date=20170522T051527Z&X-Amz-Expires=86400&X-Amz-Signature=f6c916650fe11c8ec33ed73c727a9dcfd71beac8569bc3819ca4634055ba8a38&X-Amz-SignedHeaders=host&actor_id=28613580&token=1",
                ExpiresIn = 3600,
                ExpiresAt = DateTime.Parse("2017-05-23T05:15:27Z")
            };

            var serialiser = new JsonBatchTransferSerialiser();

            var shouldBeJson = serialiser.ToString(action);
            BatchObjectAction testAction = serialiser.ObjectActionFromString(testData);
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
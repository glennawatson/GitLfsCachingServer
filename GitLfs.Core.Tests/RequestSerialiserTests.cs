// <copyright file="RequestSerialiserTests.cs" company="Glenn Watson">
// Copyright (c) 2018 Glenn Watson. All rights reserved.
// See LICENSE file in the project root for full license information.
// </copyright>

namespace GitLfs.Core.Tests
{
    using GitLfs.Core.BatchRequest;

    using Xunit;

    public class RequestSerialiserTests
    {
        [Fact]
        public void RequestDeserialiseTest()
        {
            const string jsonText =
                "{\r\n  \"operation\": \"download\",\r\n  \"transfers\": [ \"basic\" ],\r\n  \"objects\": [\r\n    {\r\n      \"oid\": \"12345678\",\r\n      \"size\": 123,\r\n    }\r\n  ]\r\n}";

            var serialiser = new JsonBatchRequestSerialiser();

            BatchRequest request = serialiser.FromString(jsonText);

            Assert.Equal(1, request.Objects.Count);
            Assert.Equal(1, request.Transfers.Count);
            Assert.Equal(BatchRequestMode.Download, request.Operation);

            Assert.Equal("12345678", request.Objects[0].Hash);
            Assert.Equal(123, request.Objects[0].Size);

            Assert.Equal(TransferMode.Basic, request.Transfers[0]);
        }

        [Fact]
        public void RequestSerialiseTest()
        {
            const string jsonText =
                "{\r\n  \"objects\": [\r\n    {\r\n      \"oid\": \"12345678\",\r\n      \"size\": 123\r\n    }\r\n  ],\r\n  \"operation\": \"download\",\r\n  \"transfers\": [\r\n    \"basic\"\r\n  ]\r\n}";

            var serialiser = new JsonBatchRequestSerialiser();

            BatchRequest objectValue = serialiser.FromString(jsonText);

            string objectText = serialiser.ToString(objectValue);

            Assert.Equal(jsonText, objectText);
        }
    }
}
// <copyright file="JsonBatchTransferSerialiser.cs" company="Glenn Watson">
// Copyright (c) 2018 Glenn Watson. All rights reserved.
// See LICENSE file in the project root for full license information.
// </copyright>

namespace GitLfs.Core.BatchResponse
{
    using System;
    using System.Collections.Generic;
    using System.Diagnostics.CodeAnalysis;
    using System.Linq;

    using Newtonsoft.Json;
    using Newtonsoft.Json.Linq;

    /// <summary>
    /// A JSON based serializer that will serialise to and from JSON strings.
    /// </summary>
    public class JsonBatchTransferSerialiser : IBatchTransferSerialiser
    {
        /// <inheritdoc />
        public BatchObjectAction ObjectActionFromString(string value)
        {
            JObject objectValue = JObject.Parse(value);

            return ProcessBatchObjectActionJson(objectValue.First as JProperty);
        }

        /// <inheritdoc />
        public BatchObject ObjectFromString(string value)
        {
            return this.ProcessBatchObjectJson(JObject.Parse(value));
        }

        /// <inheritdoc />
        [SuppressMessage("Globalization", "CA1308:Normalize strings to uppercase", Justification = "Needed by LFS standard.")]
        public string ToString(BatchTransfer transfer)
        {
            var jsonTransfer = new JObject { ["transfer"] = transfer.Mode.ToString().ToLowerInvariant() };

            var objectItemsToken = new JArray();

            jsonTransfer["objects"] = objectItemsToken;

            foreach (BatchObject objectValue in transfer.Objects.Cast<BatchObject>())
            {
                objectItemsToken.Add(this.ProcessBatchObject(objectValue));
            }

            return jsonTransfer.ToString(Formatting.Indented);
        }

        /// <inheritdoc />
        public string ToString(BatchObject batchObject)
        {
            return this.ProcessBatchObject(batchObject).ToString();
        }

        /// <inheritdoc />
        public string ToString(BatchObjectAction batchObjectAction)
        {
            JObject outerObject = new JObject
            {
                this.ProcessBatchObjectAction(batchObjectAction)
            };

            return outerObject.ToString();
        }

        /// <inheritdoc />
        public BatchTransfer TransferFromString(string value)
        {
            var transfer = new BatchTransfer { Objects = new List<IBatchObject>() };

            JObject jsonObject = JObject.Parse(value);

            if (Enum.TryParse((string)jsonObject["transfer"], true, out TransferMode mode))
            {
                transfer.Mode = mode;
            }

            foreach (JToken item in jsonObject["objects"])
            {
                IBatchObject batchObject;
                if (item["error"] != null)
                {
                    batchObject = new BatchObjectError
                        {
                            ErrorCode = (int)item["error"]["code"],
                            ErrorMessage = (string)item["error"]["message"]
                        };
                }
                else
                {
                    batchObject = this.ProcessBatchObjectJson(item);
                }

                batchObject.Id = new ObjectId((string)item["oid"], (long)item["size"]);

                transfer.Objects.Add(batchObject);
            }

            return transfer;
        }

        private static BatchObjectAction ProcessBatchObjectActionJson(JProperty actionToken)
        {
            var action = new BatchObjectAction();
            if (!Enum.TryParse(actionToken.Name, true, out BatchActionMode actionMode))
            {
                throw new ParseException("Invalid action mode.");
            }

            action.Mode = actionMode;
            action.HRef = (string)actionToken.Value["href"];
            action.ExpiresAt = (DateTime?)actionToken.Value["expires_at"];
            action.ExpiresIn = (int?)actionToken.Value["expires_in"];

            if (actionToken.Value["header"] is JObject headerToken)
            {
                var headers = new List<BatchHeader>();

                foreach (JProperty headerPair in headerToken.Cast<JProperty>())
                {
                    string key = headerPair.Name;
                    var value = (string)headerPair.Value;
                    headers.Add(new BatchHeader(key, value));
                }

                action.Headers = headers;
            }

            return action;
        }

        private JObject ProcessBatchObject(BatchObject objectValue)
        {
            var objectToken = new JObject { ["oid"] = objectValue.Id.Hash, ["size"] = objectValue.Id.Size };

            if (objectValue.Authenticated != null)
            {
                objectToken["authenticated"] = objectValue.Authenticated;
            }

            var actionsArray = new JObject();

            objectToken["actions"] = actionsArray;

            foreach (BatchObjectAction action in objectValue.Actions)
            {
                actionsArray.Add(this.ProcessBatchObjectAction(action));
            }

            return objectToken;
        }

        [SuppressMessage("Globalization", "CA1308:Normalize strings to uppercase", Justification = "Needed by LFS standard.")]
        private JProperty ProcessBatchObjectAction(BatchObjectAction action)
        {
            var actionContents = new JObject { new JProperty("href", action.HRef) };

            if (action.Headers != null)
            {
                var headers = new JObject();
                foreach (BatchHeader header in action.Headers)
                {
                    var property = new JProperty(header.Key, header.Value);
                    headers.Add(property);
                }

                actionContents["header"] = headers;
            }

            if (action.ExpiresIn != null)
            {
                actionContents.Add(new JProperty("expires_in", action.ExpiresIn));
            }

            if (action.ExpiresAt != null)
            {
                actionContents.Add(new JProperty("expires_at", action.ExpiresAt.Value.ToUniversalTime()));
            }

            return new JProperty(action.Mode.ToString().ToLowerInvariant(), actionContents);
        }

        private BatchObject ProcessBatchObjectJson(JToken item)
        {
            var batchObjectFile = new BatchObject { Authenticated = (bool?)item["authenticated"] };

            JToken actionsToken = item["actions"];

            batchObjectFile.Actions = new List<BatchObjectAction>();

            if (actionsToken != null)
            {
                foreach (JProperty actionToken in actionsToken.Cast<JProperty>())
                {
                    batchObjectFile.Actions.Add(ProcessBatchObjectActionJson(actionToken));
                }
            }

            return batchObjectFile;
        }
    }
}
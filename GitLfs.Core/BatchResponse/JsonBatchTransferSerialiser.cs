// <copyright file="JsonBatchTransferSerialiser.cs" company="Glenn Watson">
//    Copyright (C) 2017. Glenn Watson
// </copyright>

namespace GitLfs.Core.BatchResponse
{
    using System;
    using System.Collections.Generic;
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

            return this.ProcessBatchObjectActionJson(objectValue.First as JProperty);
        }

        /// <inheritdoc />
        public BatchObject ObjectFromString(string json)
        {
            return this.ProcessBatchObjectJson(JObject.Parse(json));
        }

        /// <inheritdoc />
        public string ToString(BatchTransfer transfer)
        {
            var jsonTransfer = new JObject { ["transfer"] = transfer.Mode.ToString().ToLowerInvariant() };

            var objectItemsToken = new JArray();

            jsonTransfer["objects"] = objectItemsToken;

            foreach (BatchObject objectValue in transfer.Objects)
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
            JObject outterObject = new JObject();
            outterObject.Add(this.ProcessBatchObjectAction(batchObjectAction));
            return outterObject.ToString();
        }

        /// <inheritdoc />
        public BatchTransfer TransferFromString(string json)
        {
            var transfer = new BatchTransfer { Objects = new List<IBatchObject>() };

            JObject jsonObject = JObject.Parse(json);

            if (Enum.TryParse((string)jsonObject["transfer"], true, out TransferMode mode))
            {
                transfer.Mode = mode;
            }

            foreach (JToken item in jsonObject["objects"])
            {
                IBatchObject batchObject = new BatchObject();
                if (item["error"] != null)
                {
                    var batchObjectError =
                        new BatchObjectError
                        {
                            ErrorCode = (int)item["error"]["code"],
                            ErrorMessage = (string)item["error"]["message"]
                        };
                    batchObject = batchObjectError;
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

            var actionToken = new JProperty(action.Mode.ToString().ToLowerInvariant(), actionContents);

            return actionToken;
        }

        private BatchObject ProcessBatchObjectJson(JToken item)
        {
            var batchObjectFile = new BatchObject();
            batchObjectFile.Authenticated = (bool?)item["authenticated"];

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

        private BatchObjectAction ProcessBatchObjectActionJson(JProperty actionToken)
        {
            var action = new BatchObjectAction();
            if (Enum.TryParse(actionToken.Name, true, out BatchActionMode actionMode) == false)
            {
                throw new ParseException("Invalid action mode.");
            }

            action.Mode = actionMode;
            action.HRef = (string)actionToken.Value["href"];
            action.ExpiresAt = (DateTime?)actionToken.Value["expires_at"];
            action.ExpiresIn = (int?)actionToken.Value["expires_in"];
            var headerToken = actionToken.Value["header"] as JObject;

            if (headerToken != null)
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
    }
}
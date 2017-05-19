// --------------------------------------------------------------------------------------------------------------------
// <copyright file="JsonTransferSerialiser.cs" company="Glenn Watson">
//     Copyright (C) 2017. Glenn Watson
// </copyright>
// --------------------------------------------------------------------------------------------------------------------

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
    public class JsonTransferSerialiser : ITransferSerialiser
    {
        /// <inheritdoc />
        public Transfer FromString(string json)
        {
            var transfer = new Transfer { Objects = new List<BatchObjectBase>() };

            JObject jsonObject = JObject.Parse(json);

            if (Enum.TryParse((string)jsonObject["transfer"], true, out TransferMode mode))
            {
                transfer.Mode = mode;
            }

            foreach (JToken item in jsonObject["objects"])
            {
                BatchObjectBase batchObject = new BatchObject();
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
                    var batchObjectFile = new BatchObject();
                    batchObjectFile.Authenticated = (bool?)item["authenticated"];

                    JToken actionsToken = item["actions"];

                    foreach (JProperty actionToken in actionsToken.Cast<JProperty>())
                    {
                        var action = new BatchObjectAction();
                        if (Enum.TryParse(actionToken.Name, true, out BatchActionMode actionMode) == false)
                        {
                            throw new TransferParseException("Invalid action mode.");
                        }

                        action.Mode = actionMode;
                        batchObjectFile.Actions = new List<BatchObjectAction>();
                        batchObjectFile.Actions.Add(action);
                        action.HRef = (string)actionToken.Value["href"];
                        action.ExpiresAt = (DateTime?)actionToken.Value["expires_at"];
                        action.ExpiresIn = (int?)actionToken.Value["expires_in"];
                        var headerToken = actionToken.Value["header"] as JObject;

                        if (headerToken != null)
                        {
                            IList<KeyValuePair<string, string>> headers = new List<KeyValuePair<string, string>>();

                            foreach (JProperty headerPair in headerToken.Cast<JProperty>())
                            {
                                string key = headerPair.Name;
                                var value = (string)headerPair.Value;
                                headers.Add(new KeyValuePair<string, string>(key, value));
                            }

                            action.Headers = headers;
                        }
                    }

                    batchObject = batchObjectFile;
                }

                batchObject.ObjectId = (string)item["oid"];
                batchObject.Size = (long)item["size"];

                transfer.Objects.Add(batchObject);
            }

            return transfer;
        }

        /// <inheritdoc />
        public string ToString(Transfer transfer)
        {
            var jsonTransfer = new JObject { ["transfer"] = transfer.Mode.ToString().ToLowerInvariant() };

            var objectItemsToken = new JArray();

            jsonTransfer["objects"] = objectItemsToken;

            foreach (BatchObject objectValue in transfer.Objects)
            {
                var objectToken = new JObject { ["oid"] = objectValue.ObjectId, ["size"] = objectValue.Size };

                if (objectValue.Authenticated != null)
                {
                    objectToken["authenticated"] = objectValue.Authenticated;
                }

                objectItemsToken.Add(objectToken);

                var actionsArray = new JObject();

                objectToken["actions"] = actionsArray;

                foreach (BatchObjectAction action in objectValue.Actions)
                {
                    var actionContents = new JObject { new JProperty("href", action.HRef) };

                    if (action.Headers != null)
                    {
                        var headers = new JObject();
                        foreach (KeyValuePair<string, string> header in action.Headers)
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

                    actionsArray.Add(actionToken);
                }
            }

            return jsonTransfer.ToString(Formatting.Indented);
        }
    }
}
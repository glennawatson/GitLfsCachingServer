namespace GitLfs.Core
{
    using System;
    using System.Collections.Generic;
    using System.Linq;

    using Newtonsoft.Json;
    using Newtonsoft.Json.Linq;

    public class TransferSerialiser
    {
        public static Transfer SerialiseFromJson(string json)
        {
            var transfer = new Transfer { Objects = new List<BatchObject>() };

            JObject jsonObject = JObject.Parse(json);

            if (Enum.TryParse((string)jsonObject["transfer"], true, out TransferMode mode) == false)
            {
                throw new TransferParseException("Invalid transfer object, there is no transfer mode.");
            }

            transfer.Mode = mode;

            foreach (JToken item in jsonObject["objects"])
            {
                var batchObject =
                    new BatchObject
                    {
                        ObjectId = (string)item["oid"],
                        Authenticated = (bool?)item["authenticated"],
                        Size = (long)item["size"],
                        Actions = new List<BatchObjectAction>()
                    };

                foreach (JProperty actionToken in item["actions"].Cast<JProperty>())
                {
                    var action = new BatchObjectAction();
                    if (Enum.TryParse(actionToken.Name, true, out BatchActionMode actionMode) == false)
                    {
                        throw new TransferParseException("Invalid action mode.");
                    }

                    action.Mode = actionMode;
                    batchObject.Actions.Add(action);
                    action.HRef = (string)actionToken.Value["href"];
                    action.ExpiresAt = (DateTime?)actionToken.Value["expires_at"];
                    action.ExpiresIn = (int?)actionToken.Value["expires_in"];
                    var headerToken = actionToken.Value["header"] as JObject;
                    action.Header = new List<KeyValuePair<string, string>>();

                    if (headerToken != null)
                    {
                        foreach (var headerPair in headerToken.Cast<JProperty>())
                        {
                            string key = headerPair.Name;
                            var value = (string)headerPair.Value;
                            action.Header = new KeyValuePair<string, string>("", "");

                        }
                    }
                }

                transfer.Objects.Add(batchObject);
            }

            return transfer;
        }

        public static string SerialiseToJson(Transfer transfer)
        {
            var jsonTransfer = new JObject { ["transfer"] = transfer.Mode.ToString().ToLowerInvariant() };

            var objectItemsToken = new JArray();

            jsonTransfer["objects"] = objectItemsToken;

            foreach (BatchObject objectValue in transfer.Objects)
            {
                var objectToken = new JObject
                {
                    ["oid"] = objectValue.ObjectId,
                    ["size"] = objectValue.Size,
                };

                if (objectValue.Authenticated != null)
                {
                    objectToken["authenticated"] = objectValue.Authenticated;
                }

                objectItemsToken.Add(objectToken);

                var actionsArray = new JArray();

                objectToken["actions"] = actionsArray;

                foreach (BatchObjectAction action in objectValue.Actions)
                {
                    var actionToken = new JObject { ["href"] = action.HRef };
                    if (action.ExpiresIn != null)
                    {
                        actionToken["expires_in"] = action.ExpiresIn;
                    }

                    if (action.ExpiresAt != null)
                    {
                        actionToken["expires_at"] = action.ExpiresAt.Value.ToUniversalTime().ToString("o");
                    }

                    if (action.Header != null)
                    {
                        actionToken["header"] = new JObject(action.Header.Value.Key, action.Header.Value.Value);
                    }

                    var actionOutter = new JObject(action.Mode.ToString().ToLowerInvariant(), actionToken);

                    actionsArray.Add(actionOutter);
                }
            }

            return jsonTransfer.ToString(Formatting.Indented);
        }
    }
}

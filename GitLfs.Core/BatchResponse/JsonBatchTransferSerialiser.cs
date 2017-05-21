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
    public class JsonBatchTransferSerialiser : IBatchTransferSerialiser
    {
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
                objectItemsToken.Add(ProcessBatchObject(objectValue));
			}

            return jsonTransfer.ToString(Formatting.Indented);
        }

		/// <inheritdoc />
		public string ToString(BatchObject batchObject)
        {
            return ProcessBatchObject(batchObject).ToString();
        }

        private BatchObject ProcessBatchObjectJson(JToken item)
        {
			var batchObjectFile = new BatchObject();
			batchObjectFile.Authenticated = (bool?)item["authenticated"];

			JToken actionsToken = item["actions"];

			foreach (JProperty actionToken in actionsToken.Cast<JProperty>())
			{
				var action = new BatchObjectAction();
				if (Enum.TryParse(actionToken.Name, true, out BatchActionMode actionMode) == false)
				{
					throw new ParseException("Invalid action mode.");
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
					List<BatchHeader> headers = new List<BatchHeader>();

					foreach (JProperty headerPair in headerToken.Cast<JProperty>())
					{
						string key = headerPair.Name;
						var value = (string)headerPair.Value;
						headers.Add(new BatchHeader(key, value));
					}

					action.Headers = headers;
				}
			}

            return batchObjectFile;
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

				actionsArray.Add(actionToken);
			}

            return objectToken;
        }
    }
}
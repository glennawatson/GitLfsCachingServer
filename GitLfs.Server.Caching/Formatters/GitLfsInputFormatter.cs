// --------------------------------------------------------------------------------------------------------------------
// <copyright file="GitLfsInputFormatter.cs" company="Glenn Watson">
//     Copyright (C) 2017. Glenn Watson
// </copyright>
// --------------------------------------------------------------------------------------------------------------------

namespace GitLfs.Server.Caching.Formatters
{
    using System;
    using System.IO;
    using System.Reflection;
    using System.Text;
    using System.Threading.Tasks;

    using GitLfs.Core.BatchRequest;
    using GitLfs.Core.BatchResponse;
    using GitLfs.Core.Error;

    using Microsoft.AspNetCore.Http;
    using Microsoft.AspNetCore.Mvc.Formatters;
    using Microsoft.Extensions.DependencyInjection;
    using Microsoft.Net.Http.Headers;

    using Newtonsoft.Json;

    public class GitLfsInputFormatter : TextInputFormatter
    {
        public GitLfsInputFormatter()
        {
            this.SupportedMediaTypes.Add(MediaTypeHeaderValue.Parse("application/vnd.git-lfs+json"));

            this.SupportedEncodings.Add(Encoding.UTF8);
            this.SupportedEncodings.Add(Encoding.Unicode);
        }

        /// <inheritdoc />
        public override async Task<InputFormatterResult> ReadRequestBodyAsync(
            InputFormatterContext context,
            Encoding effectiveEncoding)
        {
            if (context == null)
            {
                throw new ArgumentNullException(nameof(context));
            }

            if (effectiveEncoding == null)
            {
                throw new ArgumentNullException(nameof(effectiveEncoding));
            }

            IServiceProvider serviceProvider = context.HttpContext.RequestServices;

            var requestSerialiser = serviceProvider.GetService<IBatchRequestSerialiser>();
            var transferSerialiser = serviceProvider.GetService<IBatchTransferSerialiser>();
            var responseSerialiser = serviceProvider.GetService<IErrorResponseSerialiser>();

            HttpRequest request = context.HttpContext.Request;

            using (var reader = new StreamReader(request.Body, effectiveEncoding))
            {
                string contents = await reader.ReadToEndAsync();
                try
                {
                    BatchRequest lfsRequest = requestSerialiser.FromString(contents);
                    return await InputFormatterResult.SuccessAsync(lfsRequest);
                }
                catch (JsonException)
                {
                    try
                    {
                        ErrorResponse response = responseSerialiser.FromString(contents);
                        return await InputFormatterResult.SuccessAsync(response);
                    }
                    catch
                    {
                        try
                        {
                            BatchTransfer transfer = transferSerialiser.FromString(contents);
                            return await InputFormatterResult.SuccessAsync(transfer);
                        }
                        catch (Exception)
                        {
                            return await InputFormatterResult.FailureAsync();
                        }
                    }
                }
            }
        }

        /// <inheritdoc />
        protected override bool CanReadType(Type type)
        {
            if (type.IsAssignableFrom(typeof(BatchTransfer)) || type.IsAssignableFrom(typeof(BatchRequest)) || type.IsAssignableFrom(typeof(ErrorResponse)))
            {
                return base.CanReadType(type);
            }

            return false;
        }
    }
}
// <copyright file="GitLfsOutputFormatter.cs" company="Glenn Watson">
//     Copyright (C) 2017. Glenn Watson
// </copyright>

namespace GitLfs.Server.Caching.Formatters
{
    using System;
    using System.Reflection;
    using System.Text;
    using System.Threading.Tasks;

    using GitLfs.Core;
    using GitLfs.Core.BatchRequest;
    using GitLfs.Core.BatchResponse;
    using GitLfs.Core.Error;

    using Microsoft.AspNetCore.Http;
    using Microsoft.AspNetCore.Mvc.Formatters;
    using Microsoft.Extensions.DependencyInjection;
    using Microsoft.Net.Http.Headers;

    public class GitLfsOutputFormatter : TextOutputFormatter
    {
        public GitLfsOutputFormatter()
        {
            this.SupportedMediaTypes.Add(MediaTypeHeaderValue.Parse("application/vnd.git-lfs+json"));

            this.SupportedEncodings.Add(Encoding.UTF8);
            this.SupportedEncodings.Add(Encoding.Unicode);
        }

        /// <inheritdoc />
        public override Task WriteResponseBodyAsync(OutputFormatterWriteContext context, Encoding selectedEncoding)
        {
            IServiceProvider serviceProvider = context.HttpContext.RequestServices;
            HttpResponse response = context.HttpContext.Response;

            var requestSerialiser = serviceProvider.GetService<IBatchRequestSerialiser>();
            var transferSerialiser = serviceProvider.GetService<IBatchTransferSerialiser>();
            var responseSerialiser = serviceProvider.GetService<IErrorResponseSerialiser>();

            var transfer = context.Object as BatchTransfer;

            if (transfer != null)
            {
                string content = transferSerialiser.ToString(transfer);

                return response.WriteAsync(content);
            }

            var request = context.Object as BatchRequest;

            if (request != null)
            {
                string content = requestSerialiser.ToString(request);

                return response.WriteAsync(content);
            }

            var errorResponse = context.Object as ErrorResponse;

            if (errorResponse != null)
            {
                string content = responseSerialiser.ToString(errorResponse);

                return response.WriteAsync(content);
            }

            return response.WriteAsync(string.Empty);
        }

        /// <inheritdoc />
        protected override bool CanWriteType(Type type)
        {
            if (type.IsAssignableFrom(typeof(BatchTransfer)) || type.IsAssignableFrom(typeof(BatchRequest))
                || type.IsAssignableFrom(typeof(ErrorResponse)))
            {
                return base.CanWriteType(type);
            }

            return false;
        }
    }
}
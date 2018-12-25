// <copyright file="GitLfsOutputFormatter.cs" company="Glenn Watson">
// Copyright (c) 2018 Glenn Watson. All rights reserved.
// See LICENSE file in the project root for full license information.
// </copyright>

namespace GitLfs.Server.Caching.Formatters
{
    using System;
    using System.Reflection;
    using System.Text;
    using System.Threading.Tasks;
    using GitLfs.Core.BatchRequest;
    using GitLfs.Core.BatchResponse;
    using GitLfs.Core.ErrorHandling;
    using Microsoft.AspNetCore.Http;
    using Microsoft.AspNetCore.Mvc.Formatters;
    using Microsoft.Extensions.DependencyInjection;
    using Microsoft.Net.Http.Headers;

    /// <summary>
    /// A output formatter which handles the required files from GIT LFS.
    /// </summary>
    public class GitLfsOutputFormatter : TextOutputFormatter
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="GitLfsOutputFormatter"/> class.
        /// </summary>
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

            switch (context.Object)
            {
                case BatchTransfer transfer:
                {
                    string content = transferSerialiser.ToString(transfer);

                    return response.WriteAsync(content);
                }

                case BatchRequest request:
                {
                    string content = requestSerialiser.ToString(request);

                    return response.WriteAsync(content);
                }

                case ErrorResponse errorResponse:
                {
                    string content = responseSerialiser.ToString(errorResponse);

                    return response.WriteAsync(content);
                }

                default:
                    return response.WriteAsync(string.Empty);
            }
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
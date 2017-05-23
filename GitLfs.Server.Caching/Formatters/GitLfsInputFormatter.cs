// <copyright file="GitLfsInputFormatter.cs" company="Glenn Watson">
//    Copyright (C) 2017. Glenn Watson
// </copyright>

namespace GitLfs.Server.Caching.Formatters
{
    using System;
    using System.IO;
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

    /// <summary>
    /// A input formatter which handles the required files from GIT LFS.
    /// </summary>
    public class GitLfsInputFormatter : TextInputFormatter
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="T:GitLfs.Server.Caching.Formatters.GitLfsInputFormatter" /> class.
        /// </summary>
        public GitLfsInputFormatter()
        {
            this.SupportedMediaTypes.Add(MediaTypeHeaderValue.Parse("application/vnd.git-lfs+json"));

            this.SupportedEncodings.Add(Encoding.UTF8);
            this.SupportedEncodings.Add(Encoding.Unicode);
        }

        /// <inheritdoc />
        public override async Task<InputFormatterResult> ReadRequestBodyAsync(
            InputFormatterContext context,
            Encoding encoding)
        {
            if (context == null)
            {
                throw new ArgumentNullException(nameof(context));
            }

            if (encoding == null)
            {
                throw new ArgumentNullException(nameof(encoding));
            }

            IServiceProvider serviceProvider = context.HttpContext.RequestServices;

            var requestSerialiser = serviceProvider.GetService<IBatchRequestSerialiser>();
            var transferSerialiser = serviceProvider.GetService<IBatchTransferSerialiser>();
            var responseSerialiser = serviceProvider.GetService<IErrorResponseSerialiser>();

            HttpRequest request = context.HttpContext.Request;

            using (var reader = new StreamReader(request.Body, encoding))
            {
                string contents = await reader.ReadToEndAsync();
                try
                {
                    BatchRequest lfsRequest = requestSerialiser.FromString(contents);
                    return await InputFormatterResult.SuccessAsync(lfsRequest);
                }
                catch (ParseException)
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
                            BatchTransfer transfer = transferSerialiser.TransferFromString(contents);
                            return await InputFormatterResult.SuccessAsync(transfer);
                        }
                        catch (ParseException)
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
            if (type.IsAssignableFrom(typeof(BatchTransfer)) || type.IsAssignableFrom(typeof(BatchRequest))
                || type.IsAssignableFrom(typeof(ErrorResponse)))
            {
                return base.CanReadType(type);
            }

            return false;
        }
    }
}
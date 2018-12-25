// <copyright file="GitForwardingMiddleware.cs" company="Glenn Watson">
// Copyright (c) 2018 Glenn Watson. All rights reserved.
// See LICENSE file in the project root for full license information.
// </copyright>

namespace GitLfs.Server.Caching.Middleware
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Net.Http;
    using System.Text.RegularExpressions;
    using System.Threading.Tasks;

    using GitLfs.Core;
    using GitLfs.Server.Caching.Data;

    using Microsoft.AspNetCore.Http;
    using Microsoft.EntityFrameworkCore;
    using Microsoft.Extensions.DependencyInjection;
    using Microsoft.Extensions.Primitives;

    /// <summary>
    /// A middleware that will forward GIT requests if they aren't related to LFS.
    /// </summary>
    public sealed class GitForwardingMiddleware : IDisposable
    {
        private readonly HttpClient httpClient;

        private readonly RequestDelegate next;

        /// <summary>
        /// Initializes a new instance of the <see cref="GitForwardingMiddleware" /> class.
        /// </summary>
        /// <param name="next">The delegate representing the next middleware in the request pipeline.</param>
        public GitForwardingMiddleware(RequestDelegate next)
        {
            this.next = next;

            this.httpClient = new HttpClient();
        }

        /// <summary>
        /// Helper method to invite values async.
        /// </summary>
        /// <param name="context">The context of the current session.</param>
        /// <returns>A task to monitor the progress.</returns>
        public async Task InvokeAsync(HttpContext context)
        {
            PathString path = context.Request.Path;

            if (path.ToString().Contains("lfs", StringComparison.InvariantCulture))
            {
                await this.next(context).ConfigureAwait(false);
                return;
            }

            var matchCriteria = new Regex("/api/([0-9]+)/([a-zA-Z0-9-\\.]*/?.*)");

            Match match = matchCriteria.Match(context.Request.Path.ToString());

            if (!match.Success)
            {
                await this.next(context).ConfigureAwait(false);
                return;
            }

            var dbContext = context.RequestServices.GetService<ApplicationDbContext>();

            if (!int.TryParse(match.Groups[1].Value, out int hostIndex))
            {
                await this.next(context).ConfigureAwait(false);
                return;
            }

            GitHost gitHost = await dbContext.GitHost.SingleOrDefaultAsync(x => x.Id == hostIndex).ConfigureAwait(false);

            if (gitHost == null)
            {
                await this.next(context).ConfigureAwait(false);
                return;
            }

            var requestMessage = new HttpRequestMessage();
            string requestMethod = context.Request.Method;
            if (!HttpMethods.IsGet(requestMethod) && !HttpMethods.IsHead(requestMethod)
                && !HttpMethods.IsDelete(requestMethod) && !HttpMethods.IsTrace(requestMethod))
            {
                requestMessage.Content = new StreamContent(context.Request.Body);
            }

            // Copy the request headers
            foreach (KeyValuePair<string, StringValues> header in context.Request.Headers)
            {
                if (!requestMessage.Headers.TryAddWithoutValidation(header.Key, header.Value.ToArray()))
                {
                    requestMessage.Content?.Headers.TryAddWithoutValidation(header.Key, header.Value.ToArray());
                }
            }

            string newPath = match.Groups[2].Value;

            var uri = new Uri($"{gitHost.Href}/{context.Request.PathBase}{newPath}{context.Request.QueryString}");

            requestMessage.Headers.Host = uri.Host;
            requestMessage.RequestUri = uri;
            requestMessage.Method = new HttpMethod(context.Request.Method);
            using (HttpResponseMessage responseMessage = await this.httpClient.SendAsync(
                                                             requestMessage,
                                                             HttpCompletionOption.ResponseHeadersRead,
                                                             context.RequestAborted).ConfigureAwait(false))
            {
                context.Response.StatusCode = (int)responseMessage.StatusCode;
                foreach (KeyValuePair<string, IEnumerable<string>> header in responseMessage.Headers)
                {
                    context.Response.Headers[header.Key] = header.Value.ToArray();
                }

                foreach (KeyValuePair<string, IEnumerable<string>> header in responseMessage.Content.Headers)
                {
                    context.Response.Headers[header.Key] = header.Value.ToArray();
                }

                // SendAsync removes chunking from the response. This removes the header so it doesn't expect a chunked response.
                context.Response.Headers.Remove("transfer-encoding");
                await responseMessage.Content.CopyToAsync(context.Response.Body).ConfigureAwait(false);
            }
        }

        /// <inheritdoc />
        public void Dispose()
        {
            this.httpClient?.Dispose();
        }
    }
}
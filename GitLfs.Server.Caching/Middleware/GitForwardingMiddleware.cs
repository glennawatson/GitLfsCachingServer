namespace GitLfs.Server.Caching.Middleware
{
    using System;
    using System.Linq;
    using System.Net.Http;
    using System.Net.WebSockets;
    using System.Text.RegularExpressions;
    using System.Threading;
    using System.Threading.Tasks;

    using GitLfs.Core;
    using GitLfs.Server.Caching.Data;

    using Microsoft.AspNetCore.Builder;
    using Microsoft.AspNetCore.Http;
    using Microsoft.EntityFrameworkCore;
    using Microsoft.Extensions.DependencyInjection;
    using Microsoft.Extensions.Options;

    public class GitForwardingMiddleware
    {
        private readonly RequestDelegate next;

        private readonly HttpClient httpClient;

        /// <summary>
        /// Initializes a new instance of the <see cref="GitForwardingMiddleware"/> class.
        /// </summary>
        /// <param name="next">The delegate representing the next middleware in the request pipeline.</param>
        public GitForwardingMiddleware(RequestDelegate next)
        {
            this.next = next;

            this.httpClient = new HttpClient();
        }

        public Task Invoke(HttpContext context) => HandleHttpRequest(context);

        private async Task HandleHttpRequest(HttpContext context)
        {
            var path = context.Request.Path;

            if (path.ToString().Contains("lfs"))
            {
                await this.next(context);
                return;
            }

            Regex matchCriteria = new Regex("/api/([0-9]+)/([a-zA-Z0-9-\\.]*/?.*)");

            Match match = matchCriteria.Match(context.Request.Path.ToString());

            if (match.Success == false)
            {
                await this.next(context);
                return;
            }

            ApplicationDbContext dbContext = context.RequestServices.GetService<ApplicationDbContext>();

            if (int.TryParse(match.Groups[1].Value, out int hostIndex) == false)
            {
                await this.next(context);
                return;
            }

            GitHost gitHost = await dbContext.GitHost.SingleOrDefaultAsync(x => x.Id == hostIndex);

            if (gitHost == null)
            {
                await this.next(context);
                return;
            }

            var requestMessage = new HttpRequestMessage();
            var requestMethod = context.Request.Method;
            if (!HttpMethods.IsGet(requestMethod) &&
                !HttpMethods.IsHead(requestMethod) &&
                !HttpMethods.IsDelete(requestMethod) &&
                !HttpMethods.IsTrace(requestMethod))
            {
                var streamContent = new StreamContent(context.Request.Body);
                requestMessage.Content = streamContent;
            }

            // Copy the request headers
            foreach (var header in context.Request.Headers)
            {
                if (!requestMessage.Headers.TryAddWithoutValidation(header.Key, header.Value.ToArray()) && requestMessage.Content != null)
                {
                    requestMessage.Content?.Headers.TryAddWithoutValidation(header.Key, header.Value.ToArray());
                }
            }

            var newPath = match.Groups[2].Value;

            requestMessage.Headers.Host = "github.com";
            var uriString = $"{gitHost.Href}/{context.Request.PathBase}{newPath}{context.Request.QueryString}";
            requestMessage.RequestUri = new Uri(uriString);
            requestMessage.Method = new HttpMethod(context.Request.Method);
            using (var responseMessage = await this.httpClient.SendAsync(requestMessage, HttpCompletionOption.ResponseHeadersRead, context.RequestAborted))
            {
                context.Response.StatusCode = (int)responseMessage.StatusCode;
                foreach (var header in responseMessage.Headers)
                {
                    context.Response.Headers[header.Key] = header.Value.ToArray();
                }

                foreach (var header in responseMessage.Content.Headers)
                {
                    context.Response.Headers[header.Key] = header.Value.ToArray();
                }

                // SendAsync removes chunking from the response. This removes the header so it doesn't expect a chunked response.
                context.Response.Headers.Remove("transfer-encoding");
                await responseMessage.Content.CopyToAsync(context.Response.Body);
            }
        }
    }
}

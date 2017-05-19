namespace GitLfs.Server.Caching.Formatters
{
    using System;
    using System.Reflection;
    using System.Text;
    using System.Threading.Tasks;

    using GitLfs.Core.BatchRequest;
    using GitLfs.Core.BatchResponse;

    using Microsoft.AspNetCore.Http;
    using Microsoft.AspNetCore.Mvc.Formatters;
    using Microsoft.Extensions.DependencyInjection;
    using Microsoft.Net.Http.Headers;

    public class GitLfsOutputFormatter : TextOutputFormatter
    {
        public GitLfsOutputFormatter()
        {
            SupportedMediaTypes.Add(MediaTypeHeaderValue.Parse("application/vnd.git-lfs+json"));

            SupportedEncodings.Add(Encoding.UTF8);
            SupportedEncodings.Add(Encoding.Unicode);
        }

        /// <inheritdoc />
        public override Task WriteResponseBodyAsync(OutputFormatterWriteContext context, Encoding selectedEncoding)
        {
            IServiceProvider serviceProvider = context.HttpContext.RequestServices;
            HttpResponse response = context.HttpContext.Response;

            IRequestSerialiser requestSerialiser = serviceProvider.GetService<IRequestSerialiser>();
            ITransferSerialiser transferSerialiser = serviceProvider.GetService<ITransferSerialiser>();

            Transfer transfer = context.Object as Transfer;

            if (transfer != null)
            {
                var content = transferSerialiser.ToString(transfer);

                return response.WriteAsync(content);
            }

            Request request = context.Object as Request;

            if (request != null)
            {
                var content = requestSerialiser.ToString(request);

                return response.WriteAsync(content);
            }

            return response.WriteAsync(string.Empty);
        }

        /// <inheritdoc />
        protected override bool CanWriteType(Type type)
        {
            if (type.IsAssignableFrom(typeof(Transfer)) || type.IsAssignableFrom(typeof(Request)))
            {
                return base.CanWriteType(type);
            }

            return false;
        }
    }
}

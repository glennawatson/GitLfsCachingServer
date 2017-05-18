namespace GitLfs.Server.Caching.Middleware
{
    using System;

    using Microsoft.AspNetCore.Builder;

    /// <summary>
    /// Extension methods for the <see cref="GitForwardingBuilderExtensions"/>
    /// </summary>
    public static class GitForwardingBuilderExtensions
    {
        /// <summary>
        /// Checks if a given Url matches rules and conditions, and modifies the HttpContext on match.
        /// </summary>
        /// <param name="app">The <see cref="IApplicationBuilder"/></param>
        /// <returns>The application builder.</returns>
        public static IApplicationBuilder UseGitForwarding(this IApplicationBuilder app)
        {
            if (app == null)
            {
                throw new ArgumentNullException(nameof(app));
            }

            // put middleware in pipeline
            return app.UseMiddleware<GitForwardingMiddleware>();
        }
    }
}

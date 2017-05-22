// <copyright file="EnsureMigration.cs" company="Glenn Watson">
//      Copyright (C) 2017. Glenn Watson
// </copyright>

namespace GitLfs.Server.Caching.Extensions
{
    using Microsoft.AspNetCore.Builder;
    using Microsoft.EntityFrameworkCore;
    using Microsoft.Extensions.DependencyInjection;

    /// <summary>
    /// Automatically will migrate the database context with the latest version.
    /// </summary>
    public static class EnsureMigration
    {
        /// <summary>
        /// A extension menthod that will trigger the database migration if needed.
        /// </summary>
        /// <typeparam name="T">The type of the database context.</typeparam>
        /// <param name="app">The application we are going to get the DB instance from.</param>
        public static void EnsureMigrationOfContext<T>(this IApplicationBuilder app)
            where T : DbContext
        {
            var context = app.ApplicationServices.GetService<T>();
            context.Database.Migrate();
        }
    }
}
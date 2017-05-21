// --------------------------------------------------------------------------------------------------------------------
// <copyright file="ApplicationDbContext.cs" company="Glenn Watson">
//     Copyright (C) 2017. Glenn Watson
// </copyright>
// --------------------------------------------------------------------------------------------------------------------

namespace GitLfs.Server.Caching.Data
{
    using GitLfs.Core;
    using GitLfs.Core.BatchResponse;
    using GitLfs.Server.Caching.Models;

    using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
    using Microsoft.EntityFrameworkCore;

    /// <summary>
    /// The main database context for the application. Includes all the authentication
    /// data in the base class. 
    /// </summary>
    public class ApplicationDbContext : IdentityDbContext<ApplicationUser>
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="T:GitLfs.Server.Caching.Data.ApplicationDbContext"/> class.
        /// </summary>
        /// <param name="options">The options to use when generating the database..</param>
        public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options)
            : base(options)
        {
        }

        /// <summary>
        /// Gets or sets git hosts that are associated with the web server.
        /// </summary>
        /// <value>The git host.</value>
        public DbSet<GitHost> GitHost { get; set; }

        /// <inheritdoc />
        protected override void OnModelCreating(ModelBuilder builder)
        {
            builder.Entity<GitHost>().HasIndex(x => x.Name).IsUnique();

            base.OnModelCreating(builder);
        }
    }
}
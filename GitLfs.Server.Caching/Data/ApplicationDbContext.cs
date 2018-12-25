// <copyright file="ApplicationDbContext.cs" company="Glenn Watson">
// Copyright (c) 2018 Glenn Watson. All rights reserved.
// See LICENSE file in the project root for full license information.
// </copyright>

namespace GitLfs.Server.Caching.Data
{
    using System;
    using System.Collections.Generic;
    using System.Text;
    using GitLfs.Core;
    using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
    using Microsoft.EntityFrameworkCore;

    /// <summary>
    /// The main database context for the application. Includes all the authentication
    /// data in the base class.
    /// </summary>
    public class ApplicationDbContext : IdentityDbContext
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="ApplicationDbContext"/> class.
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

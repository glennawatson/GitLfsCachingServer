// --------------------------------------------------------------------------------------------------------------------
// <copyright file="ApplicationDbContext.cs" company="Glenn Watson">
//     Copyright (C) 2017. Glenn Watson
// </copyright>
// --------------------------------------------------------------------------------------------------------------------

namespace GitLfs.Server.Caching.Data
{
    using GitLfs.Core;
    using GitLfs.Server.Caching.Models;

    using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
    using Microsoft.EntityFrameworkCore;

    public class ApplicationDbContext : IdentityDbContext<ApplicationUser>
    {
        public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options)
            : base(options)
        {
        }

        public DbSet<GitHost> GitHost { get; set; }

        public DbSet<GitLfsFile> LfsFiles { get; set; }

        protected override void OnModelCreating(ModelBuilder builder)
        {
            builder.Entity<GitHost>().HasIndex(x => x.Name).IsUnique();
            builder.Entity<GitLfsFile>().HasIndex(x => x.ObjectId).IsUnique();

            base.OnModelCreating(builder);
        }
    }
}
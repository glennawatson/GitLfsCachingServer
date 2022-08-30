// <copyright file="Startup.cs" company="Glenn Watson">
// Copyright (c) 2018 Glenn Watson. All rights reserved.
// See LICENSE file in the project root for full license information.
// </copyright>

namespace GitLfs.Server.Caching;

using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.IO;
using System.Linq;
using System.Text.Json.Serialization;
using System.Threading.Tasks;
using GitLfs.Client;
using GitLfs.Core.BatchRequest;
using GitLfs.Core.BatchResponse;
using GitLfs.Core.ErrorHandling;
using GitLfs.Core.File;
using GitLfs.Core.Verify;
using GitLfs.Server.Caching.Data;
using GitLfs.Server.Caching.Formatters;
using GitLfs.Server.Caching.Middleware;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.HttpsPolicy;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Identity.UI;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Newtonsoft.Json;

/// <summary>
/// The startup object where we register all our details about our web site.
/// </summary>
public class Startup
{
    /// <summary>
    /// Initializes a new instance of the <see cref="Startup" /> class.
    /// </summary>
    /// <param name="configuration">The configuration for the server..</param>
    public Startup(IConfiguration configuration)
    {
        this.Configuration = configuration;
    }

    /// <summary>
    /// Gets the configuration.
    /// </summary>
    public IConfiguration Configuration { get; }

    /// <summary>
    /// This method gets called by the runtime. Use this method to add services to the container.
    /// </summary>
    /// <param name="services">A collection of services, where we register our service information.</param>
    public void ConfigureServices(IServiceCollection services)
    {
        services.Configure<CookiePolicyOptions>(options =>
        {
            // This lambda determines whether user consent for non-essential cookies is needed for a given request.
            options.CheckConsentNeeded = _ => true;
            options.MinimumSameSitePolicy = SameSiteMode.None;
        });

        services.Configure<MvcOptions>(options => options.EnableEndpointRouting = false);

        services.AddDbContext<ApplicationDbContext>(options =>
            options.UseSqlite(
                this.Configuration.GetConnectionString("DefaultConnection")));
        services.AddDefaultIdentity<IdentityUser>()
            .AddDefaultUI()
            .AddEntityFrameworkStores<ApplicationDbContext>();

        services.AddMvc(
            options =>
            {
                options.OutputFormatters.Insert(0, new GitLfsOutputFormatter());
                options.InputFormatters.Insert(0, new GitLfsInputFormatter());
            })
            .AddJsonOptions(options => options.JsonSerializerOptions.DefaultIgnoreCondition = JsonIgnoreCondition.Always);

        services.AddHttpClient();

        services.AddSingleton<IBatchRequestSerialiser>(new JsonBatchRequestSerialiser());
        services.AddSingleton<IBatchTransferSerialiser>(new JsonBatchTransferSerialiser());
        services.AddSingleton<IErrorResponseSerialiser>(new JsonErrorResponseSerialiser());
        services.AddSingleton<IVerifyObjectSerialiser>(new JsonVerifyObjectSerialiser());
        services.AddSingleton<IFileManager, LfsFileManager>();
        services.AddTransient<ILfsClient, FileCachingLfsClient>();
    }

    /// <summary>
    /// This method gets called by the runtime. Use this method to configure the HTTP request pipeline.
    /// </summary>
    /// <param name="app">Application where we can register different settings.</param>
    /// <param name="env">The hosting environment details.</param>
    [SuppressMessage("Documentation", "CA1822: Use static method", Justification = "Used by ASP.NET Reflection")]
    public void Configure(IApplicationBuilder app, IWebHostEnvironment env)
    {
        if (Directory.Exists(env.ContentRootPath + "/lfs/meta"))
        {
            Directory.Delete(env.ContentRootPath + "/lfs/meta", true);
        }

        if (Directory.Exists(env.ContentRootPath + "/lfs/temp"))
        {
            Directory.Delete(env.ContentRootPath + "/lfs/temp", true);
        }

        if (env.IsDevelopment())
        {
            app.UseDeveloperExceptionPage();
        }
        else
        {
            app.UseExceptionHandler("/Home/Error");

            // The default HSTS value is 30 days. You may want to change this for production scenarios, see https://aka.ms/aspnetcore-hsts.
            app.UseHsts();
        }

        //// Reenable if you have a valid SSL certificate.
        //// app.UseHttpsRedirection();

        app.UseStaticFiles();
        app.UseCookiePolicy();

        app.UseAuthentication();

        app.UseGitForwarding();

        app.UseMvc(routes =>
        {
            routes.MapRoute(
                name: "default",
                template: "{controller=Home}/{action=Index}/{id?}");
        });
    }
}

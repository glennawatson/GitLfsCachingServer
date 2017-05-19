// --------------------------------------------------------------------------------------------------------------------
// <copyright file="Startup.cs" company="Glenn Watson">
//     Copyright (C) 2017. Glenn Watson
// </copyright>
// --------------------------------------------------------------------------------------------------------------------

namespace GitLfs.Server.Caching
{
    using System.IO;
    using System.Threading.Tasks;

    using GitLfs.Client;
    using GitLfs.Core.BatchRequest;
    using GitLfs.Core.BatchResponse;
    using GitLfs.Core.Error;
    using GitLfs.Core.Managers;
    using GitLfs.Server.Caching.Data;
    using GitLfs.Server.Caching.Formatters;
    using GitLfs.Server.Caching.Middleware;
    using GitLfs.Server.Caching.Models;
    using GitLfs.Server.Caching.Services;

    using Microsoft.AspNetCore.Authentication.Cookies;
    using Microsoft.AspNetCore.Builder;
    using Microsoft.AspNetCore.Hosting;
    using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
    using Microsoft.EntityFrameworkCore;
    using Microsoft.Extensions.Configuration;
    using Microsoft.Extensions.DependencyInjection;
    using Microsoft.Extensions.Logging;

    using Newtonsoft.Json;

    /// <summary>
    /// The startup object where we register all our details about our web site.
    /// </summary>
    public class Startup
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="Startup" /> class.
        /// </summary>
        /// <param name="env">Details about our environment for the web server.</param>
        public Startup(IHostingEnvironment env)
        {
            IConfigurationBuilder builder = new ConfigurationBuilder().SetBasePath(env.ContentRootPath)
                .AddJsonFile("appsettings.json", false, true)
                .AddJsonFile($"appsettings.{env.EnvironmentName}.json", true);

            if (env.IsDevelopment())
            {
                builder.AddUserSecrets<Startup>();
            }

            builder.AddEnvironmentVariables();
            this.Configuration = builder.Build();
        }

        /// <summary>
        /// Gets the configuration.
        /// </summary>
        public IConfigurationRoot Configuration { get; }

        /// <summary>
        /// This method gets called by the runtime. Use this method to configure the HTTP request pipeline.
        /// </summary>
        /// <param name="app">Application where we can register different settings.</param>
        /// <param name="env">The hosting environment details.</param>
        /// <param name="loggerFactory">A logging factory where we can register logging details.</param>
        public void Configure(IApplicationBuilder app, IHostingEnvironment env, ILoggerFactory loggerFactory)
        {
            loggerFactory.AddConsole(this.Configuration.GetSection("Logging"));
            loggerFactory.AddDebug();

            //if (env.IsDevelopment())
            //{
            //    app.UseDeveloperExceptionPage();
            //    app.UseDatabaseErrorPage();
            //    app.UseBrowserLink();
            //}
            //else
            //{
            //    app.UseExceptionHandler("/Home/Error");
            //}

            app.UseStaticFiles();

            app.UseIdentity();

            app.UseGitForwarding();

            // Add external authentication middle ware below. To configure them please see https://go.microsoft.com/fwlink/?LinkID=532715
            app.UseMvc(
                routes =>
                    {
                        routes.MapRoute("default", "{controller=Home}/{action=Index}/{id?}");
                    });
        }

        /// <summary>
        /// This method gets called by the runtime. Use this method to add services to the container.
        /// </summary>
        /// <param name="services">A collection of services, where we register our service information.</param>
        public void ConfigureServices(IServiceCollection services)
        {
            // Add framework services.
            services.AddDbContext<ApplicationDbContext>(
                options => options.UseSqlite(this.Configuration.GetConnectionString("DefaultConnection")));

            // Disable auto redirect on the api based syntax.
            services.AddIdentity<ApplicationUser, IdentityRole>(
                    options =>
                        {
                            options.Cookies.ApplicationCookie.Events = new CookieAuthenticationEvents
                                                                          {
                                                                              OnRedirectToLogin = ctx =>
                                                                                  {
                                                                                      if (!ctx.Request.Path.StartsWithSegments("/api"))
                                                                                      {
                                                                                          ctx.Response.Redirect(ctx.RedirectUri);
                                                                                      }

                                                                                      return Task.FromResult(0);
                                                                                  }
                                                                          };
                        })
                .AddEntityFrameworkStores<ApplicationDbContext>()
                .AddDefaultTokenProviders();

            services
                .AddMvc(
                    options =>
                        {
                            options.OutputFormatters.Add(new GitLfsOutputFormatter());
                            options.InputFormatters.Add(new GitLfsInputFormatter());
                        })
                .AddJsonOptions(options => options.SerializerSettings.NullValueHandling = NullValueHandling.Ignore);

            // Add application services.
            services.AddTransient<IEmailSender, AuthMessageSender>();
            services.AddTransient<ISmsSender, AuthMessageSender>();
            services.AddSingleton<IBatchRequestSerialiser>(new JsonBatchRequestSerialiser());
            services.AddSingleton<IBatchTransferSerialiser>(new JsonBatchTransferSerialiser());
            services.AddSingleton<IErrorResponseSerialiser>(new JsonErrorResponseSerialiser());
            services.AddSingleton<IFileManager, LfsFileManager>();
            services.AddTransient<ILfsClient, FileCachingLfsClient>();
        }
    }
}
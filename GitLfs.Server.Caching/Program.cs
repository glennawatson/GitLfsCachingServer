// <copyright file="Program.cs" company="Glenn Watson">
//    Copyright (C) 2017. Glenn Watson
// </copyright>

namespace GitLfs.Server.Caching
{
    using System.IO;

    using Microsoft.AspNetCore.Hosting;
    using Microsoft.Extensions.Configuration;

    /// <summary>
    /// The main class that will be invoked when the web application starts.
    /// </summary>
    public class Program
    {
        /// <summary>
        /// The main entry into the web service.
        /// </summary>
        /// <param name="args">The arguments passed to the applications.</param>
        public static void Main(string[] args)
        {
            var config = new ConfigurationBuilder()
                .AddCommandLine(args)
                .Build();

            IWebHost host = new WebHostBuilder()
                .UseConfiguration(config)
                .CaptureStartupErrors(true)
                .UseKestrel()
                .UseContentRoot(Directory.GetCurrentDirectory())
                .UseIISIntegration()
                .UseStartup<Startup>()
                // .UseApplicationInsights()
                .Build();

            host.Run();
        }
    }
}
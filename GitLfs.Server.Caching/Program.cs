// --------------------------------------------------------------------------------------------------------------------
// <copyright file="Program.cs" company="Glenn Watson">
//     Copyright (C) 2017. Glenn Watson
// </copyright>
// --------------------------------------------------------------------------------------------------------------------

namespace GitLfs.Server.Caching
{
    using System.IO;

    using Microsoft.AspNetCore.Hosting;

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
            IWebHost host = new WebHostBuilder()
                .UseUrls("http://*:5000")
                .UseKestrel()
                .UseContentRoot(Directory.GetCurrentDirectory())
                .UseIISIntegration()
                .UseStartup<Startup>()
                //.UseApplicationInsights()
                .Build();

            host.Run();
        }
    }
}
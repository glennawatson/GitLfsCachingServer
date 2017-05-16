// --------------------------------------------------------------------------------------------------------------------
// <copyright file="Program.cs" company="Glenn Watson">
//     Copyright (C) 2017. Glenn Watson
// </copyright>
// --------------------------------------------------------------------------------------------------------------------

namespace GitLfs.Server.Caching
{
    using System.IO;
    using System.Security.Cryptography.X509Certificates;

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
            var cert = new X509Certificate2("test.pfx",
                "test");
            IWebHost host = new WebHostBuilder()
                .UseKestrel(cfg => cfg.UseHttps(cert))
                .UseContentRoot(Directory.GetCurrentDirectory())
                .UseIISIntegration()
                .UseStartup<Startup>()
                .UseApplicationInsights()
                .Build();

            host.Run();
        }
    }
}
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
            var host = new WebHostBuilder()
                .UseKestrel()
                .UseContentRoot(Directory.GetCurrentDirectory())
                .UseIISIntegration()
                .UseStartup<Startup>()
                .UseApplicationInsights()
                .Build();

            host.Run();
        }
    }
}

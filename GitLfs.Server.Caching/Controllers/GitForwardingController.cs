namespace GitLfs.Server.Caching.Controllers
{
    using System.Threading.Tasks;

    using GitLfs.Core;
    using GitLfs.Server.Caching.Data;

    using Microsoft.AspNetCore.Mvc;

    public class GitForwardingController : Controller
    {
        private readonly ApplicationDbContext context;

        public GitForwardingController(ApplicationDbContext context)
        {
            this.context = context;
        }

        [Route("/api/{hostId}/{repositoryName}/{*elseText}")]
        public async Task<IActionResult> Forward(int hostId, string repositoryName, string elseText)
        {
            GitHost host = await this.context.GitHost.FindAsync(hostId);

            if (host == null)
            {
                return this.NotFound(new ErrorResponse { Message = "Not a valid host id." });
            }

            return this.Redirect($"{host.Href}/{elseText}");
        }
    }
}
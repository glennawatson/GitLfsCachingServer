// --------------------------------------------------------------------------------------------------------------------
// <copyright file="LfsController.cs" company="Glenn Watson">
//     Copyright (C) 2017. Glenn Watson
// </copyright>
// --------------------------------------------------------------------------------------------------------------------

namespace GitLfs.Server.Caching.Controllers
{
    using System.Threading.Tasks;

    using GitLfs.Core;
    using GitLfs.Core.BatchRequest;
    using GitLfs.Server.Caching.Data;

    using Microsoft.AspNetCore.Mvc;
    using Microsoft.EntityFrameworkCore;

    /// <summary>
    /// Controller for handling LFS data.
    /// </summary>
    [Produces("application/json")]
    [Route("api/Lfs")]
    public class LfsController : Controller
    {
        private readonly ApplicationDbContext context;

        private readonly IRequestSerialiser requestSerialiser;

        public LfsController(ApplicationDbContext context, IRequestSerialiser requestSerialiser)
        {
            this.context = context;
            this.requestSerialiser = requestSerialiser;
        }

        [HttpGet("{repositoryName}/lfs/info/{objectId}")]
        public async Task<IActionResult> DownloadFile(string repositoryName, string objectId)
        {
            var repository = this.context.GitRepository.FirstOrDefaultAsync(x => x.Name == repositoryName);

            if (repository == null)
            {
                return this.NotFound(new ErrorResponse() { Message = "Not a valid repository name."});
            }

            Request transferRequest = this.requestSerialiser.FromString(requestData);


        }
    }
}
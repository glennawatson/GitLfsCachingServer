// <copyright file="GitHostsController.cs" company="Glenn Watson">
//    Copyright (C) 2017. Glenn Watson
// </copyright>

namespace GitLfs.Server.Caching.Controllers
{
    using System.Linq;
    using System.Threading.Tasks;

    using GitLfs.Core;
    using GitLfs.Server.Caching.Data;

    using Microsoft.AspNetCore.Authorization;
    using Microsoft.AspNetCore.Mvc;
    using Microsoft.EntityFrameworkCore;

    /// <summary>
    /// Hosts controller for the git hosts.
    /// </summary>
    [Authorize]
    public class GitHostsController : Controller
    {
        private readonly ApplicationDbContext context;

        /// <summary>
        /// Initializes a new instance of the GitHostsController class.
        /// </summary>
        /// <param name="context">The database context to allow changing of the database.</param>
        public GitHostsController(ApplicationDbContext context)
        {
            this.context = context;
        }

        /// <summary>
        /// Creates a new view for creating a GitHost object.
        /// </summary>
        /// <returns>A action to monitor the progress.</returns>
        public IActionResult Create()
        {
            return this.View();
        }

        /// <summary>
        /// Creates a new GitHost.
        /// To protect from overposting attacks, please enable the specific properties you want to bind to, for
        /// more details see http://go.microsoft.com/fwlink/?LinkId=317598.
        /// </summary>
        /// <param name="gitHost">The new GitHost object to create.</param>
        /// <returns>A action result with the status.</returns>
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create([Bind("Id,Href,Name,UserName,Token")] GitHost gitHost)
        {
            if (this.ModelState.IsValid)
            {
                this.context.Add(gitHost);
                await this.context.SaveChangesAsync();
                return this.RedirectToAction("Index");
            }

            return this.View(gitHost);
        }

        /// <summary>
        /// Deltes a GitHost object from the database with the specified ID.
        /// </summary>
        /// <param name="id">The ID to delete.</param>
        /// <returns>A action result to monitor the progress.</returns>
        public async Task<IActionResult> Delete(int? id)
        {
            if (id == null)
            {
                return this.NotFound();
            }

            GitHost gitHost = await this.context.GitHost.SingleOrDefaultAsync(m => m.Id == id);
            if (gitHost == null)
            {
                return this.NotFound();
            }

            return this.View(gitHost);
        }

        /// <summary>
        /// Confirm screen to show that the object has been deleted.
        /// </summary>
        /// <param name="id">The ID of the item that has been deleted.</param>
        /// <returns>A result to monitor the progress.</returns>
        [HttpPost]
        [ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            GitHost gitHost = await this.context.GitHost.SingleOrDefaultAsync(m => m.Id == id);
            this.context.GitHost.Remove(gitHost);
            await this.context.SaveChangesAsync();
            return this.RedirectToAction("Index");
        }

        /// <summary>
        /// Shows a view with the details of the specified GitHost id.
        /// </summary>
        /// <param name="id">The ID to get the details for.</param>
        /// <returns>A task to monitor the progress.</returns>
        public async Task<IActionResult> Details(int? id)
        {
            if (id == null)
            {
                return this.NotFound();
            }

            GitHost gitHost = await this.context.GitHost.SingleOrDefaultAsync(m => m.Id == id);
            if (gitHost == null)
            {
                return this.NotFound();
            }

            return this.View(gitHost);
        }

        /// <summary>
        /// Allows editing of the specified GitHost using the specified ID.
        /// </summary>
        /// <param name="id">The ID of the GitHost object.</param>
        /// <returns>A action result to monitor the progress.</returns>
        public async Task<IActionResult> Edit(int? id)
        {
            if (id == null)
            {
                return this.NotFound();
            }

            GitHost gitHost = await this.context.GitHost.SingleOrDefaultAsync(m => m.Id == id);
            if (gitHost == null)
            {
                return this.NotFound();
            }

            return this.View(gitHost);
        }

        // POST: GitHosts/Edit/5
        // To protect from overposting attacks, please enable the specific properties you want to bind to, for 
        // more details see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, [Bind("Id,Href,Name,UserName,Token")] GitHost gitHost)
        {
            if (id != gitHost.Id)
            {
                return this.NotFound();
            }

            if (this.ModelState.IsValid)
            {
                try
                {
                    this.context.Update(gitHost);
                    await this.context.SaveChangesAsync();
                }
                catch (DbUpdateConcurrencyException)
                {
                    if (!this.GitHostExists(gitHost.Id))
                    {
                        return this.NotFound();
                    }

                    throw;
                }

                return this.RedirectToAction("Index");
            }

            return this.View(gitHost);
        }

        /// <summary>
        /// Shows the main view where it lists the GitHost objects in the Database.
        /// </summary>
        /// <returns>A task to monitor the progress.</returns>
        public async Task<IActionResult> Index()
        {
            return this.View(await this.context.GitHost.ToListAsync());
        }

        private bool GitHostExists(int id)
        {
            return this.context.GitHost.Any(e => e.Id == id);
        }
    }
}
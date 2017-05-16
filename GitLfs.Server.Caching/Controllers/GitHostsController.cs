// --------------------------------------------------------------------------------------------------------------------
// <copyright file="GitHostsController.cs" company="Glenn Watson">
//     Copyright (C) 2017. Glenn Watson
// </copyright>
// --------------------------------------------------------------------------------------------------------------------

namespace GitLfs.Server.Caching.Controllers
{
    using System.Linq;
    using System.Threading.Tasks;

    using GitLfs.Core;
    using GitLfs.Server.Caching.Data;

    using Microsoft.AspNetCore.Authorization;
    using Microsoft.AspNetCore.Mvc;
    using Microsoft.EntityFrameworkCore;

    [Authorize]
    public class GitHostsController : Controller
    {
        private readonly ApplicationDbContext context;

        public GitHostsController(ApplicationDbContext context)
        {
            this.context = context;
        }

        // GET: GitHosts/Create
        public IActionResult Create()
        {
            return this.View();
        }

        // POST: GitHosts/Create
        // To protect from overposting attacks, please enable the specific properties you want to bind to, for 
        // more details see http://go.microsoft.com/fwlink/?LinkId=317598.
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

        // GET: GitHosts/Delete/5
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

        // POST: GitHosts/Delete/5
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

        // GET: GitHosts/Details/5
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

        // GET: GitHosts/Edit/5
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

        // GET: GitHosts
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
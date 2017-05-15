// --------------------------------------------------------------------------------------------------------------------
// <copyright file="GitRepositoriesController.cs" company="Glenn Watson">
//     Copyright (C) 2017. Glenn Watson
// </copyright>
// --------------------------------------------------------------------------------------------------------------------

namespace GitLfs.Server.Caching.Controllers
{
    using System.Linq;
    using System.Threading.Tasks;

    using GitLfs.Server.Caching.Data;
    using GitLfs.Server.Caching.Models;

    using Microsoft.AspNetCore.Authorization;
    using Microsoft.AspNetCore.Mvc;
    using Microsoft.EntityFrameworkCore;

    [Authorize]
    public class GitRepositoriesController : Controller
    {
        private readonly ApplicationDbContext context;

        public GitRepositoriesController(ApplicationDbContext context)
        {
            this.context = context;
        }

        // GET: GitRepositories/Create
        public IActionResult Create()
        {
            return this.View();
        }

        // POST: GitRepositories/Create
        // To protect from overposting attacks, please enable the specific properties you want to bind to, for 
        // more details see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create([Bind("Id,Href,Name,UserName,Password")] GitRepository gitRepository)
        {
            if (this.ModelState.IsValid)
            {
                this.context.Add(gitRepository);
                await this.context.SaveChangesAsync();
                return this.RedirectToAction("Index");
            }

            return this.View(gitRepository);
        }

        // GET: GitRepositories/Delete/5
        public async Task<IActionResult> Delete(int? id)
        {
            if (id == null)
            {
                return this.NotFound();
            }

            GitRepository gitRepository = await this.context.GitRepository.SingleOrDefaultAsync(m => m.Id == id);
            if (gitRepository == null)
            {
                return this.NotFound();
            }

            return this.View(gitRepository);
        }

        // POST: GitRepositories/Delete/5
        [HttpPost]
        [ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            GitRepository gitRepository = await this.context.GitRepository.SingleOrDefaultAsync(m => m.Id == id);
            this.context.GitRepository.Remove(gitRepository);
            await this.context.SaveChangesAsync();
            return this.RedirectToAction("Index");
        }

        // GET: GitRepositories/Details/5
        public async Task<IActionResult> Details(int? id)
        {
            if (id == null)
            {
                return this.NotFound();
            }

            GitRepository gitRepository = await this.context.GitRepository.SingleOrDefaultAsync(m => m.Id == id);
            if (gitRepository == null)
            {
                return this.NotFound();
            }

            return this.View(gitRepository);
        }

        // GET: GitRepositories/Edit/5
        public async Task<IActionResult> Edit(int? id)
        {
            if (id == null)
            {
                return this.NotFound();
            }

            GitRepository gitRepository = await this.context.GitRepository.SingleOrDefaultAsync(m => m.Id == id);
            if (gitRepository == null)
            {
                return this.NotFound();
            }

            return this.View(gitRepository);
        }

        // POST: GitRepositories/Edit/5
        // To protect from overposting attacks, please enable the specific properties you want to bind to, for 
        // more details see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(
            int id,
            [Bind("Id,Href,Name,UserName,Password")] GitRepository gitRepository)
        {
            if (id != gitRepository.Id)
            {
                return this.NotFound();
            }

            if (this.ModelState.IsValid)
            {
                try
                {
                    this.context.Update(gitRepository);
                    await this.context.SaveChangesAsync();
                }
                catch (DbUpdateConcurrencyException)
                {
                    if (!this.GitRepositoryExists(gitRepository.Id))
                    {
                        return this.NotFound();
                    }

                    throw;
                }

                return this.RedirectToAction("Index");
            }

            return this.View(gitRepository);
        }

        // GET: GitRepositories
        public async Task<IActionResult> Index()
        {
            return this.View(await this.context.GitRepository.ToListAsync());
        }

        private bool GitRepositoryExists(int id)
        {
            return this.context.GitRepository.Any(e => e.Id == id);
        }
    }
}
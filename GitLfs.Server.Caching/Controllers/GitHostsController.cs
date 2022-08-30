// <copyright file="GitHostsController.cs" company="Glenn Watson">
// Copyright (c) 2018 Glenn Watson. All rights reserved.
// See LICENSE file in the project root for full license information.
// </copyright>

namespace GitLfs.Server.Caching.Controllers;

using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using GitLfs.Core;
using GitLfs.Server.Caching.Data;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;

/// <summary>
/// Hosts controller for the git hosts.
/// </summary>
[Authorize]
public class GitHostsController : Controller
{
    private readonly ApplicationDbContext _context;

    /// <summary>
    /// Initializes a new instance of the <see cref="GitHostsController"/> class.
    /// </summary>
    /// <param name="context">The database context to allow changing of the database.</param>
    public GitHostsController(ApplicationDbContext context)
    {
        this._context = context;
    }

    /// <summary>
    /// Shows the main view where it lists the GitHost objects in the Database.
    /// </summary>
    /// <returns>The result of the action.</returns>
    public async Task<IActionResult> Index()
    {
        return this.View(await this._context.GitHost.ToListAsync().ConfigureAwait(false));
    }

    /// <summary>
    /// Brings up the details about a git host.
    /// </summary>
    /// <param name="id">The ID of the git host to bring up.</param>
    /// <returns>The result of the action.</returns>
    public async Task<IActionResult> Details(int? id)
    {
        if (id == null)
        {
            return this.NotFound();
        }

        var gitHost = await this._context.GitHost.FirstOrDefaultAsync(m => m.Id == id).ConfigureAwait(false);
        if (gitHost == null)
        {
            return this.NotFound();
        }

        return this.View(gitHost);
    }

    /// <summary>
    /// Starts the creation of a new git host value.
    /// </summary>
    /// <returns>The result of the action.</returns>
    public IActionResult Create()
    {
        return this.View();
    }

    /// <summary>
    /// Creates a new GitHost.
    /// To protect from over-posting attacks, please enable the specific properties you want to bind to, for
    /// more details see http://go.microsoft.com/fwlink/?LinkId=317598.
    /// </summary>
    /// <param name="gitHost">The new GitHost object to create.</param>
    /// <returns>A action result with the status.</returns>
    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Create([Bind("Href,Id,Name,Token,UserName")] GitHost gitHost)
    {
        if (this.ModelState.IsValid)
        {
            this._context.Add(gitHost);
            await this._context.SaveChangesAsync().ConfigureAwait(false);
            return this.RedirectToAction(nameof(this.Index));
        }

        return this.View(gitHost);
    }

    /// <summary>
    /// Edits the specified git host with the provided values.
    /// </summary>
    /// <param name="id">The id of the host to edit.</param>
    /// <returns>The result of the action.</returns>
    public async Task<IActionResult> Edit(int? id)
    {
        if (id == null)
        {
            return this.NotFound();
        }

        var gitHost = await this._context.GitHost.FindAsync(id).ConfigureAwait(false);
        if (gitHost == null)
        {
            return this.NotFound();
        }

        return this.View(gitHost);
    }

    /// <summary>
    /// Edits the specified git host with the provided values.
    /// </summary>
    /// <param name="id">The id of the host to edit.</param>
    /// <param name="gitHost">The values of the new git host values.</param>
    /// <returns>The result of the action.</returns>
    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Edit(int id, [Bind("Href,Id,Name,Token,UserName")] GitHost gitHost)
    {
        if (id != gitHost.Id)
        {
            return this.NotFound();
        }

        if (this.ModelState.IsValid)
        {
            try
            {
                this._context.Update(gitHost);
                await this._context.SaveChangesAsync().ConfigureAwait(false);
            }
            catch (DbUpdateConcurrencyException)
            {
                if (!this.GitHostExists(gitHost.Id))
                {
                    return this.NotFound();
                }

                throw;
            }

            return this.RedirectToAction(nameof(this.Index));
        }

        return this.View(gitHost);
    }

    /// <summary>
    /// Deletes a GitHost object from the database with the specified ID.
    /// </summary>
    /// <param name="id">The ID to delete.</param>
    /// <returns>A action result to monitor the progress.</returns>
    public async Task<IActionResult> Delete(int? id)
    {
        if (id == null)
        {
            return this.NotFound();
        }

        var gitHost = await this._context.GitHost
            .FirstOrDefaultAsync(m => m.Id == id).ConfigureAwait(false);
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
        var gitHost = await this._context.GitHost.FindAsync(id).ConfigureAwait(false);
        this._context.GitHost.Remove(gitHost);
        await this._context.SaveChangesAsync().ConfigureAwait(false);
        return this.RedirectToAction(nameof(this.Index));
    }

    private bool GitHostExists(int id)
    {
        return this._context.GitHost.Any(e => e.Id == id);
    }
}

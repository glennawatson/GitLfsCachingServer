// <copyright file="HomeController.cs" company="Glenn Watson">
// Copyright (c) 2018 Glenn Watson. All rights reserved.
// See LICENSE file in the project root for full license information.
// </copyright>

namespace GitLfs.Server.Caching.Controllers;

using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Threading.Tasks;
using GitLfs.Server.Caching.Models;
using Microsoft.AspNetCore.Mvc;

/// <summary>
/// The main controller for the application where the user will initially hit.
/// </summary>
public class HomeController : Controller
{
    /// <summary>
    /// Gets the main index page for the user.
    /// </summary>
    /// <returns>The result of the action.</returns>
    public IActionResult Index()
    {
        return this.View();
    }

    /// <summary>
    /// Gets the main error response page for the user.
    /// </summary>
    /// <returns>The result of the action.</returns>
    [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
    public IActionResult Error()
    {
        return this.View(new ErrorViewModel { RequestId = Activity.Current?.Id ?? this.HttpContext.TraceIdentifier });
    }
}

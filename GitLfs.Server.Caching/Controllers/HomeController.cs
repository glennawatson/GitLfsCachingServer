// <copyright file="HomeController.cs" company="Glenn Watson">
//    Copyright (C) 2017. Glenn Watson
// </copyright>

namespace GitLfs.Server.Caching.Controllers
{
    using Microsoft.AspNetCore.Mvc;

    public class HomeController : Controller
    {
        public IActionResult About()
        {
            this.ViewData["Message"] = "Your application description page.";

            return this.View();
        }

        public IActionResult Contact()
        {
            this.ViewData["Message"] = "Your contact page.";

            return this.View();
        }

        public IActionResult Error()
        {
            return this.View();
        }

        public IActionResult GitRepositories()
        {
            this.ViewData["Message"] = "GIT Repositories";

            return this.View();
        }

        public IActionResult Index()
        {
            return this.View();
        }
    }
}
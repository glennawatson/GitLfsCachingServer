// <copyright file="SendCodeViewModel.cs" company="Glenn Watson">
//    Copyright (C) 2017. Glenn Watson
// </copyright>

namespace GitLfs.Server.Caching.Models.AccountViewModels
{
    using System.Collections.Generic;

    using Microsoft.AspNetCore.Mvc.Rendering;

    public class SendCodeViewModel
    {
        public ICollection<SelectListItem> Providers { get; set; }

        public bool RememberMe { get; set; }

        public string ReturnUrl { get; set; }

        public string SelectedProvider { get; set; }
    }
}
// <copyright file="IndexViewModel.cs" company="Glenn Watson">
//    Copyright (C) 2017. Glenn Watson
// </copyright>

namespace GitLfs.Server.Caching.Models.ManageViewModels
{
    using System.Collections.Generic;

    using Microsoft.AspNetCore.Identity;

    public class IndexViewModel
    {
        public bool BrowserRemembered { get; set; }

        public bool HasPassword { get; set; }

        public IList<UserLoginInfo> Logins { get; set; }

        public string PhoneNumber { get; set; }

        public bool TwoFactor { get; set; }
    }
}
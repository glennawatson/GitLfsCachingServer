// --------------------------------------------------------------------------------------------------------------------
// <copyright file="RemoveLoginViewModel.cs" company="Glenn Watson">
//     Copyright (C) 2017. Glenn Watson
// </copyright>
// --------------------------------------------------------------------------------------------------------------------

namespace GitLfs.Server.Caching.Models.ManageViewModels
{
    public class RemoveLoginViewModel
    {
        public string LoginProvider { get; set; }

        public string ProviderKey { get; set; }
    }
}
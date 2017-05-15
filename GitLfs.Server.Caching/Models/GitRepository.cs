// --------------------------------------------------------------------------------------------------------------------
// <copyright file="GitRepository.cs" company="Glenn Watson">
//     Copyright (C) 2017. Glenn Watson
// </copyright>
// --------------------------------------------------------------------------------------------------------------------

namespace GitLfs.Server.Caching.Models
{
    /// <summary>
    /// A repository for GIT.
    /// </summary>
    public class GitRepository
    {
        /// <summary>
        /// Gets or sets the base HREF for the git repository.
        /// </summary>
        public string Href { get; set; }

        /// <summary>
        /// Gets or sets the name of the GIT repository.
        /// </summary>
        public string Name { get; set; }
    }
}
// --------------------------------------------------------------------------------------------------------------------
// <copyright file="GitRepository.cs" company="Glenn Watson">
//     Copyright (C) 2017. Glenn Watson
// </copyright>
// --------------------------------------------------------------------------------------------------------------------

namespace GitLfs.Server.Caching.Models
{
    using System.ComponentModel;
    using System.ComponentModel.DataAnnotations;

    /// <summary>
    /// A repository for GIT.
    /// </summary>
    public class GitRepository
    {
        /// <summary>
        /// Gets or sets the Id of the git repository.
        /// </summary>
        public int Id { get; set; }

        /// <summary>
        /// Gets or sets the base HREF for the git repository.
        /// </summary>
        [Required]
        [Url]
        [DisplayName("Repository URL")]
        public string Href { get; set; }

        /// <summary>
        /// Gets or sets the name of the GIT repository.
        /// </summary>
        [Required]
        [DisplayName("Repository Name")]
        public string Name { get; set; }

        /// <summary>
        /// Gets or sets the user name on the GIT repository.
        /// </summary>
        [Required]
        [DisplayName("Repository User Name")]
        public string UserName { get; set; }

        /// <summary>
        /// Gets or sets the password on the GIT repository.
        /// </summary>
        [Required]
        [DataType(DataType.Password)]
        [DisplayName("Repository Password")]
        public string Password { get; set; }
    }
}
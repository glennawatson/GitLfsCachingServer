// <copyright file="GitHost.cs" company="Glenn Watson">
//    Copyright (C) 2017. Glenn Watson
// </copyright>

namespace GitLfs.Core
{
    using System.ComponentModel;
    using System.ComponentModel.DataAnnotations;

    /// <summary>
    /// A repository for GIT.
    /// </summary>
    public class GitHost
    {
        /// <summary>
        /// Gets or sets the base HREF for the git repository.
        /// </summary>
        [Required]
        [Url]
        [DisplayName("Host URL")]
        public string Href { get; set; }

        /// <summary>
        /// Gets or sets the Id of the git repository.
        /// </summary>
        public int Id { get; set; }

        /// <summary>
        /// Gets or sets the name of the GIT repository.
        /// </summary>
        [Required]
        [DisplayName("Host Name")]
        public string Name { get; set; }

        /// <summary>
        /// Gets or sets the password on the GIT repository.
        /// </summary>
        [Required]
        [DataType(DataType.Password)]
        [DisplayName("Host Authentication Token")]
        public string Token { get; set; }

        /// <summary>
        /// Gets or sets the user name on the GIT repository.
        /// </summary>
        [Required]
        [DisplayName("Host User Name")]
        public string UserName { get; set; }
    }
}
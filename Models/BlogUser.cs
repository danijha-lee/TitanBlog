using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Identity;

namespace TitanBlog.Models
{
    public class BlogUser : IdentityUser
    {
        // internal BlogUser modUser;

        [Required]
        [Display(Name = "First Name")]
        [StringLength(40, ErrorMessage = "The {0} must be at least {2} and at most {1} characters long.", MinimumLength = 5)]
        public string FirstName { get; set; }

        [Required]
        [Display(Name = "Last Name")]
        [StringLength(40, ErrorMessage = "The {0} must be at least {2} and at most {1} characters long.", MinimumLength = 5)]
        public string LastName { get; set; }

        [Required]
        [Display(Name = "Display Name")]
        [StringLength(40, ErrorMessage = "The {0} must be at least {2} and at most {1} characters long.", MinimumLength = 5)]
        public string DisplayName { get; set; }

        public byte[] ImageData { get; internal set; }
        public string ImageType { get; internal set; }

        [NotMapped]
        public string FullName
        {
            get
            {
                return $"{FirstName} {LastName}";
            }
        }
    }
}
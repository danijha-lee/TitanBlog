using System;
using System.ComponentModel.DataAnnotations;
using Microsoft.AspNetCore.Identity;
using TitanBlog.Enums;

namespace TitanBlog.Models
{
    public class Comment
    {
        public int Id { get; set; }
        public int PostId { get; set; }
        public string AuthorId { get; set; }
        public string ModeratorId { get; set; }

        [DataType(DataType.Date)]
        public DateTime Created { get; set; }

        [DataType(DataType.Date)]
        public DateTime? Updated { get; set; }

        [DataType(DataType.Date)]
        public DateTime? Moderated { get; set; }

        public DateTime? Deleted { get; set; }

        [Required]
        [Display(Name = "Comment (500 characters or less)")]
        [StringLength(500, ErrorMessage = "The {0} must be at least {2} and at most {1} characters long", MinimumLength = 1)]
        public string Body { get; set; }

        [Display(Name = "Moderated Comment")]
        [StringLength(500, ErrorMessage = "The {0} must be at least {2} and at most {1} characters long", MinimumLength = 1)]
        public string ModeratedBody { get; set; }

        public ModerationType ModerationReason { get; set; }

        //Navigational Properties
        public virtual Post Post { get; set; }

        public virtual BlogUser Author { get; set; }
        public virtual BlogUser Moderator { get; set; }
    }
}
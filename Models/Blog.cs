using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;

namespace TitanBlog.Models
{
    public class Blog
    {
        public int Id { get; set; }
        public string AuthorId { get; set; }

        [Required]
        [StringLength(100, ErrorMessage ="The {0} must be at least {2} and at most {1} characters long.", MinimumLength =2)]
        public string Name { get; set; }

        [Required]
        [Display(Name = "Description (500 characters or less")]
        [StringLength(500, ErrorMessage = "The {0} must be at least {2} and at most {1} characters long.", MinimumLength = 5)]
        public string Description { get; set; }

        [DataType(DataType.Date)]
        [Display(Name ="Created Date")]
        public DateTime Created { get; set; }


        [DataType(DataType.Date)]
        [Display(Name = "Updated Date")]
        public DateTime? Updated { get; set; }

      [Display(Name ="Select Image")]
      [NotMapped]
        public IFormFile Image { get; set; }
        public string ImageType { get; set; }

        public byte[] ImageData { get; set; }

        public virtual ICollection<Post> Posts { get; set; } = new HashSet<Post>();
        public virtual BlogUser Author { get; set;  }

    }
}

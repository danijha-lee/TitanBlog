using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Threading.Tasks;

namespace TitanBlog.Models
{
    public class Tag
    {
        public int Id { get; set; }
        public int PostId { get; set; }
        public string AuthorId { get; set; }

        [Required]
        [StringLength(25, ErrorMessage = "The {0} must be at least {2} and no more than {1} characters long ", MinimumLength = 2)]
        public string Text { get; set; }


        // Navigational properties
        public virtual ICollection<Post> Posts { get; set; } = new HashSet<Post>();
    }
}

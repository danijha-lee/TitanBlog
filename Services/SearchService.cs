using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using TitanBlog.Data;
using TitanBlog.Models;

namespace TitanBlog.Services
{
    public class SearchService
    {
        private readonly ApplicationDbContext _context;

        public SearchService(ApplicationDbContext context)
        {
            _context = context;
        }

        public IQueryable<Post> ContentSearch(string searchStr)
        {
            var posts = _context.Posts.Where(p => p.IsReady);
            if (!string.IsNullOrEmpty(searchStr))
            {
                searchStr = searchStr.ToLower();
                posts = posts.Where(p =>
                p.Title.Contains(searchStr) ||
                p.Abstract.Contains(searchStr) ||
                p.Content.Contains(searchStr) ||
                p.Comments.Any(c =>
                   c.Body.Contains(searchStr) ||
                  c.ModeratedBody.Contains(searchStr) ||
                  c.Author.FirstName.Contains(searchStr) ||
                  c.Author.LastName.Contains(searchStr) ||
                   c.Author.Email.Contains(searchStr)));
            }
            return posts.OrderByDescending(p => p.Created);
        }
    }
}
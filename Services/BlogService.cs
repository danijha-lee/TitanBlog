using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using TitanBlog.Data;
using TitanBlog.Models;

namespace TitanBlog.Services
{
    public class BlogService
    {
        private readonly ApplicationDbContext _context;

        public BlogService(ApplicationDbContext context)
        {
            _context = context;
        }

        public async Task<List<Blog>> GetIndexPosts()
        {
            return await _context.Blogs.OrderByDescending(p => p.Created).ToListAsync();
        }
    }
}
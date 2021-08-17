using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using TitanBlog.Data;
using TitanBlog.Models;

namespace TitanBlog.Services
{
    public class PostService
    {
        private readonly ApplicationDbContext _context;

        public PostService(ApplicationDbContext context)
        {
            _context = context;
        }

        public async Task<List<Post>> GetIndexPosts()
        {
            return await _context.Posts.Where(p => p.IsReady).OrderByDescending(p => p.Created).ToListAsync();
        }
    }
}
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using TitanBlog.Data;
using TitanBlog.Models;

namespace TitanBlog.Services
{
    public class CommentService
    {
        private readonly ApplicationDbContext _context;

        public CommentService(ApplicationDbContext context)
        {
            _context = context;
        }

        public async Task<List<Comment>> GetIndexComments()
        {
            return await _context.Comments.Where(c => c.Deleted != null).ToListAsync();
        }

        public async Task<List<Comment>> GetDeletedComments()
        {
            return await _context.Comments.Where(c => c.Deleted == null).ToListAsync();
        }

        public async Task<List<Comment>> GetModeratedComments(string reason)
        {
            return await _context.Comments.Where(c => c.ModerationReason.ToString() == reason).ToListAsync();
        }
    }
}
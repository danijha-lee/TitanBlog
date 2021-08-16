using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using TitanBlog.Data;
using TitanBlog.Models;
using TitanBlog.Services;
using TitanBlog.Services.Interfaces;

namespace TitanBlog.Controllers
{
    public class PostsController : Controller
    {
        private readonly ApplicationDbContext _context;
        private readonly BasicSlugService _slugService;
        private readonly IImageService _imageService;
        private readonly SearchService _searchService;

        public PostsController(ApplicationDbContext context, BasicSlugService slugService, IImageService imageService, SearchService searchService)
        {
            _context = context;
            _slugService = slugService;
            _imageService = imageService;
            _searchService = searchService;
        }

        // GET: Posts
        public async Task<IActionResult> Index()
        {
            var applicationDbContext = _context.Posts.Where(p => p.IsReady);

            return View(await applicationDbContext.ToListAsync());
        }

        public async Task<IActionResult> BlogPostIndex(int? blogId)
        {
            if (blogId is null)
            {
                return NotFound();
            }

            var posts = await _context.Posts
                .Where(p => p.BlogId == blogId && p.IsReady)
                .ToListAsync();

            return View(posts);
        }

        public IActionResult SearchIndex(string searchStr)
        {
            var posts = _searchService.ContentSearch(searchStr);
            return View("BlogPostIndex", posts);
        }

        [Authorize(Roles = "Administrator")]
        public async Task<IActionResult> AdminIndex()
        {
            var applicationDbContext = _context.Posts.Include(p => p.Blog);

            return View(await applicationDbContext.ToListAsync());
        }

        // GET: Posts/Details/5
        [Authorize(Roles = "Administrator")]
        public async Task<IActionResult> Details(string slug)
        {
            if (string.IsNullOrEmpty(slug))
            {
                return NotFound();
            }

            var post = await _context.Posts
                .Include(p => p.Blog).Include(p => p.Comments.Where(p => p.Deleted == null))
                .ThenInclude(c => c.Author)
                .FirstOrDefaultAsync(p => p.Slug == slug);

            if (post == null)
            {
                return NotFound();
            }

            return View(post);
        }

        // GET: Posts/Create
        [Authorize(Roles = "Administrator")]
        public IActionResult Create()
        {
            ViewData["BlogId"] = new SelectList(_context.Blogs, "Id", "Name");
            return View();
        }

        // POST: Posts/Create
        // To protect from overposting attacks, enable the specific properties you want to bind to.
        // For more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
        [Authorize(Roles = "Administrator")]
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create([Bind("BlogId,Title,Abstract,Content,Image,IsReady")] Post post)
        {
            if (ModelState.IsValid)
            {
                post.ImageType = _imageService.ContentType(post.Image);
                post.ImageData = await _imageService.EncodeImageAsync(post.Image);

                var slug = _slugService.UrlFriendly(post.Title);

                //I can only allow this new Post to be saved IF the slug is unique
                if (_slugService.IsUnique(slug))
                {
                    post.Slug = slug;
                }
                else
                {
                    //I have determined that the Title results in a duplicate Slug...
                    ModelState.AddModelError("Title", "The Title you entered cannot be used please try again");
                    ModelState.AddModelError("", "There was an error related to the Title...");
                    ViewData["BlogId"] = new SelectList(_context.Blogs, "Id", "Name", post.BlogId);
                    return View(post);
                }

                post.Created = DateTime.Now;

                _context.Add(post);
                await _context.SaveChangesAsync();
                return RedirectToAction(nameof(Index));
            }
            ViewData["BlogId"] = new SelectList(_context.Blogs, "Id", "Name", post.BlogId);
            return View(post);
        }

        // GET: Posts/Edit/5
        [Authorize(Roles = "Administrator")]
        public async Task<IActionResult> Edit(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var post = await _context.Posts.FindAsync(id);
            if (post == null)
            {
                return NotFound();
            }
            ViewData["BlogId"] = new SelectList(_context.Blogs, "Id", "Description", post.BlogId);
            return View(post);
        }

        // POST: Posts/Edit/5
        // To protect from overposting attacks, enable the specific properties you want to bind to.
        // For more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
        [Authorize(Roles = "Administrator")]
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, [Bind("Id,BlogId,Title,Abstract,Created,Content,Slug,IsReady,Image,ImageData,ImageType")] Post post)
        {
            if (id != post.Id)
            {
                return NotFound();
            }

            if (ModelState.IsValid)
            {
                try
                {
                    var newImageData = await _imageService.EncodeImageAsync(post.Image);

                    if (post.ImageData != newImageData && post.Image != null)
                    {
                        post.ImageType = _imageService.ContentType(post.Image);
                        post.ImageData = newImageData;
                    }

                    var slug = _slugService.UrlFriendly(post.Title);

                    //slightly more optimized code, only updating post.Slug when needed.
                    //Added to make myself remember that there are more optimized ways of doing
                    //The same thing.
                    if (slug != post.Slug)
                    {
                        if (_slugService.IsUnique(slug))
                        {
                            post.Slug = slug;
                        }
                        else
                        {
                            //I have determined that the Title results in a duplicate Slug...
                            ModelState.AddModelError("Title", "The Title you entered cannot be used please try again");
                            ModelState.AddModelError("", "There was an error related to the Title...");

                            ViewData["BlogId"] = new SelectList(_context.Blogs, "Id", "Name", post.BlogId);
                            return View(post);
                        }
                    }

                    post.Updated = DateTime.Now;

                    _context.Update(post);
                    await _context.SaveChangesAsync();
                }
                catch (DbUpdateConcurrencyException)
                {
                    if (!PostExists(post.Id))
                    {
                        return NotFound();
                    }
                    else
                    {
                        throw;
                    }
                }
                return RedirectToAction(nameof(Index));
            }
            ViewData["BlogId"] = new SelectList(_context.Blogs, "Id", "Name", post.BlogId);
            return View(post);
        }

        // GET: Posts/Delete/5
        [Authorize(Roles = "Administrator")]
        public async Task<IActionResult> Delete(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var post = await _context.Posts
                .Include(p => p.Blog)
                .FirstOrDefaultAsync(m => m.Id == id);
            if (post == null)
            {
                return NotFound();
            }

            return View(post);
        }

        // POST: Posts/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            var post = await _context.Posts.FindAsync(id);
            _context.Posts.Remove(post);
            await _context.SaveChangesAsync();
            return RedirectToAction(nameof(Index));
        }

        private bool PostExists(int id)
        {
            return _context.Posts.Any(e => e.Id == id);
        }
    }
}

//using System;
//using System.Collections.Generic;
//using System.Linq;
//using System.Threading.Tasks;
//using Microsoft.AspNetCore.Mvc;
//using Microsoft.AspNetCore.Mvc.Rendering;
//using Microsoft.EntityFrameworkCore;
//using TitanBlog.Data;
//using TitanBlog.Models;
//using TitanBlog.Services;

//namespace TitanBlog.Controllers
//{
//    public class PostsController : Controller
//    {
//        private readonly ApplicationDbContext _context;
//        private readonly BasicSlugService _slugService;
//        private readonly IImageservice _imageService;

//        public PostsController(ApplicationDbContext context, BasicSlugService slugService, IImageservice imageService)
//        {
//            _context = context;
//            _slugService = slugService;
//            _imageService = imageService;
//        }

//        // GET: Posts
//        public async Task<IActionResult> Index()
//        {
//            var applicationDbContext = _context.Posts.Where(p => p.IsReady);
//            return View(await applicationDbContext.ToListAsync());
//        }
//        public async Task<IActionResult>  AdminIndex()
//        {
//            var applicationDbContext = _context.Posts.Include(p => p.Blog);
//            return View("Index",await applicationDbContext.ToListAsync());
//        }

//        public async Task <IActionResult> BlogPostIndex(int? blogId)
//        {
//            if (blogId is null)
//            {
//                return NotFound();
//            }

//            var posts = await _context.Posts
//                 .Where(p => p.BlogId == blogId && p.IsReady)
//                 .ToListAsync();
//            return View(posts);

//        }

//        // GET: Posts/Details/5
//        public async Task<IActionResult> Details(string slug)
//        {
//            if (string.IsNullOrEmpty(slug))
//            {
//                return NotFound();
//            }

//            var post = await _context.Posts
//                 .Include(p => p.Blog).Include(p => p.Comments)
//                 .ThenInclude(c => c.Author)
//                 .FirstOrDefaultAsync(p => p.Slug == slug);

//            if (post == null)
//            {
//                return NotFound();
//            }

//            return View(post);
//        }

//        // GET: Posts/Create
//        public IActionResult Create()
//        {
//            ViewData["BlogId"] = new SelectList(_context.Blogs, "Id", "Name");

//            return View();
//        }

//        // POST: Posts/Create
//        // To protect from overposting attacks, enable the specific properties you want to bind to.
//        // For more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
//        [HttpPost]
//        [ValidateAntiForgeryToken]
//        public async Task<IActionResult> Create([Bind("BlogId,Image,Title,Abstract,Content,IsReady")] Post post)
//        {
//            if (ModelState.IsValid)
//            {
//                post.ImageType = _imageService.ContentType(post.Image);
//                post.ImageData = await _imageService.EncodeImageAsync(post.Image);

//                var slug = _slugService.UrlFriendly(post.Title);

//                //I can only allow this new Post to be saved IF the slug is unique
//                if (_slugService.IsUnique(slug))
//                {
//                    post.Slug = slug;
//                }
//                else
//                {
//                    //I have determined that the Title results in a duplicate Slug...
//                    ModelState.AddModelError("Title", "The Title you entered cannot be used please try again");
//                    ModelState.AddModelError("", "There was an error related to the Title...");
//                    ViewData["BlogId"] = new SelectList(_context.Blogs, "Id", "Name", post.BlogId);
//                    return View(post);
//                }

//                post.Created = DateTime.Now;
//                _context.Add(post);
//                await _context.SaveChangesAsync();
//                return RedirectToAction(nameof(Index));
//            }
//            ViewData["BlogId"] = new SelectList(_context.Blogs, "Id", "Name", post.BlogId);
//            return View(post);
//        }

//        // GET: Posts/Edit/5
//        public async Task<IActionResult> Edit(int? id)
//        {
//            if (id == null)
//            {
//                return NotFound();
//            }

//            var post = await _context.Posts.FindAsync(id);
//            if (post == null)
//            {
//                return NotFound();
//            }
//            ViewData["BlogId"] = new SelectList(_context.Blogs, "Id", "Name", post.BlogId);
//            return View(post);
//        }

//        // POST: Posts/Edit/5
//        // To protect from overposting attacks, enable the specific properties you want to bind to.
//        // For more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
//        [HttpPost]
//        [ValidateAntiForgeryToken]
//        public async Task<IActionResult> Edit(int id, [Bind("Id,BlogId,Title,Abstract,Content,Created,Image,ImageDate,ImageType,IsReady,Slug")] Post post)
//        {
//            if (id != post.Id)
//            {
//                return NotFound();
//            }

//            if (ModelState.IsValid)
//            {
//                try
//                {
//                    var newImageData = await _imageService.EncodeImageAsync(post.Image);

//                    if (post.ImageData != newImageData && post.Image != null)
//                    {
//                        post.ImageType = _imageService.ContentType(post.Image);
//                        post.ImageData = newImageData;
//                    }

//                    var slug = _slugService.UrlFriendly(post.Title);
//                    if (slug != post.Slug)
//                    {
//                        if (_slugService.IsUnique(slug))
//                        {
//                            post.Slug = slug;
//                        }
//                        else
//                        {
//                            //I have determined that the Title results in a duplicate Slug...
//                            ModelState.AddModelError("Title", "The Title you entered cannot be used please try again");
//                            ModelState.AddModelError("", "There was an error related to the Title...");

//                            ViewData["BlogId"] = new SelectList(_context.Blogs, "Id", "Name", post.BlogId);
//                            return View(post);
//                        }
//                    }

//                    post.Updated = DateTime.Now;
//                    //I need to compare the current slug to the previous
//                    //Slug to determine if I have to check for uniqueness
//                    _context.Update(post);
//                    await _context.SaveChangesAsync();
//                }
//                catch (DbUpdateConcurrencyException)
//                {
//                    if (!PostExists(post.Id))
//                    {
//                        return NotFound();
//                    }
//                    else
//                    {
//                        throw;
//                    }
//                }
//                return RedirectToAction(nameof(Index));
//            }
//            ViewData["BlogId"] = new SelectList(_context.Blogs, "Id", "Name", post.BlogId);
//            return View(post);
//        }

//        // GET: Posts/Delete/5
//        public async Task<IActionResult> Delete(int? id)
//        {
//            if (id == null)
//            {
//                return NotFound();
//            }

//            var post = await _context.Posts
//                .Include(p => p.Blog)
//                .FirstOrDefaultAsync(m => m.Id == id);
//            if (post == null)
//            {
//                return NotFound();
//            }

//            return View(post);
//        }

//        // POST: Posts/Delete/5
//        [HttpPost, ActionName("Delete")]
//        [ValidateAntiForgeryToken]
//        public async Task<IActionResult> DeleteConfirmed(int id)
//        {
//            var post = await _context.Posts.FindAsync(id);
//            _context.Posts.Remove(post);
//            await _context.SaveChangesAsync();
//            return RedirectToAction(nameof(Index));
//        }

//        private bool PostExists(int id)
//        {
//            return _context.Posts.Any(e => e.Id == id);
//        }
//    }
//}
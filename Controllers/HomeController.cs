using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Identity.UI.Services;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using TitanBlog.Data;
using TitanBlog.Models;

namespace TitanBlog.Controllers
{
    public class HomeController : Controller
    {
        private readonly ILogger<HomeController> _logger;
        private readonly IEmailSender _emailSender;
        private readonly IConfiguration _configuration;
        private readonly ApplicationDbContext _context;

        public HomeController(ILogger<HomeController> logger, IEmailSender emailSender, IConfiguration configuration, ApplicationDbContext context)
        {
            _logger = logger;
            _emailSender = emailSender;
            _configuration = configuration;
            _context = context;
        }

        public async Task<IActionResult> Index()
        {
            return View(await _context.Blogs.ToListAsync());
        }

        public IActionResult About()
        {
            return View();
        }

        public IActionResult ContactMe()
        {
            return View();
        }

        [HttpPost]
        public IActionResult ContactMe([Bind("Name, Email, Subject, Message")] Contact contact)
        {
            var emailTo = _configuration["MailSettings:Email"];

            var emailBody = new StringBuilder();

            emailBody.AppendLine($"From: {contact.Name} <br />");
            emailBody.AppendLine($"{contact.Email} <br /><br />");
            emailBody.AppendLine(contact.Message);

            _emailSender.SendEmailAsync(emailTo, contact.Subject, emailBody.ToString());
            return RedirectToAction("Index", "Home");
        }

        [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
        public IActionResult Error()
        {
            return View(new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
        }
    }
}
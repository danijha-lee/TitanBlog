using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using TitanBlog.Data;
using TitanBlog.Models;

namespace TitanBlog.Services
{
    public class BasicSeedService
    {
        private readonly ApplicationDbContext _context;
        private readonly RoleManager<IdentityRole> _roleManager;
        private readonly UserManager<BlogUser> _userManager;

        public BasicSeedService(ApplicationDbContext context, RoleManager<IdentityRole> roleManager, UserManager<BlogUser> userManager)
        {
            _context = context;
            _roleManager = roleManager;
            _userManager = userManager;
        }

        public async Task ManageDataAsync()
        {
            await _context.Database.MigrateAsync();
            await SeedDataAsync();
        }

        //This is a wrapper method
        public async Task SeedDataAsync()
        {
            await SeedRolesAsync();
            await SeedUsersAsync();
        }

        private async Task SeedRolesAsync()
        {
            //Task 1: Ask the Database if any Roles already exist
            if (_context.Roles.Any())
            {
                return;
            }
            // Task 2: create the necessary Roles if they don't already exist
            await _roleManager.CreateAsync(new IdentityRole("Administrator"));
            await _roleManager.CreateAsync(new IdentityRole("Moderator"));
        }

        private async Task SeedUsersAsync()
        {
            //Task 1: Ask the Db if any Users already exist
            if (_context.Users.Any()) { return; }

            //Task 2: Creat the User intended to occupy the Administrator role
            var adminUser = new BlogUser()
            {
                Email = "danijha.monaelee@gmail.com",
                UserName = "danijha.monaelee@gmail.com",
                FirstName = "Monae",
                LastName = "Lee",
                DisplayName = "MonaeLee",
                PhoneNumber = "3364948074",
                EmailConfirmed = true,
            };

            await _userManager.CreateAsync(adminUser, "Prettyface669!");
            await _userManager.AddToRoleAsync(adminUser, "Administrator");

            var modUser = new BlogUser()
            {
                Email = "themonaelee@gmail.com",
                UserName = "themonaelee@gmail.com",
                FirstName = "Moderator (Monae)",
                LastName = "Lee",
                DisplayName = "Moderator",
                PhoneNumber = "3364948074",
                EmailConfirmed = true
            };

            await _userManager.CreateAsync(modUser, "Prettyface669!");
            await _userManager.AddToRoleAsync(modUser, "Moderator");
        }
    }
}
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using TitanBlog.Data;
using TitanBlog.Models;
using TitanBlog.Services.Interfaces;

namespace TitanBlog.Areas.Identity.Pages.Account.Manage
{
    public partial class IndexModel : PageModel
    {
        private readonly UserManager<BlogUser> _userManager;
        private readonly SignInManager<BlogUser> _signInManager;
        private readonly IImageService _imageService;
        private readonly ApplicationDbContext _context;

        public IndexModel(
            UserManager<BlogUser> userManager,
            SignInManager<BlogUser> signInManager, IImageService imageService, ApplicationDbContext context)
        {
            _userManager = userManager;
            _signInManager = signInManager;
            _imageService = imageService;
            _context = context;
        }

        #region Props

        public string Username { get; set; }
        public string CurrentImageData { get; set; }

        public string ImageType { get; set; }
        public byte[] ImageData { get; set; }

        [TempData]
        public string StatusMessage { get; set; }

        [TempData]
        public string ErrorMessage { get; set; }

        [BindProperty]
        public InputModel Input { get; set; }

        #endregion Props

        public class InputModel
        {
            [Phone]
            [Display(Name = "Phone number")]
            public string PhoneNumber { get; set; }

            [Display(Name = "Select a custom image")]
            public IFormFile Image { get; set; }
        }

        private async Task LoadAsync(BlogUser user)
        {
            var userName = await _userManager.GetUserNameAsync(user);
            var phoneNumber = await _userManager.GetPhoneNumberAsync(user);

            Username = userName;
            CurrentImageData = _imageService.DecodeImage(user.ImageData, user.ImageType);
            ImageType = user.ImageType;
            ImageData = user.ImageData;

            Input = new InputModel
            {
                PhoneNumber = phoneNumber
            };
        }

        public async Task<IActionResult> OnGetAsync()
        {
            var user = await _userManager.GetUserAsync(User);
            if (user == null)
            {
                return NotFound($"Unable to load user with ID '{_userManager.GetUserId(User)}'.");
            }

            await LoadAsync(user);
            return Page();
        }

        public async Task<IActionResult> OnPostAsync()
        {
            var user = await _userManager.GetUserAsync(User);
            if (user == null)
            {
                return NotFound($"Unable to load user with ID '{_userManager.GetUserId(User)}'.");
            }

            if (!ModelState.IsValid)
            {
                await LoadAsync(user);
                return Page();
            }

            //Make sure the user has selected an approved image type

            var phoneNumber = await _userManager.GetPhoneNumberAsync(user);
            if (Input.PhoneNumber != phoneNumber)
            {
                var setPhoneResult = await _userManager.SetPhoneNumberAsync(user, Input.PhoneNumber);
                if (!setPhoneResult.Succeeded)
                {
                    StatusMessage = "Unexpected error when trying to set phone number.";
                    return RedirectToPage();
                }
            }
            var newImageData = await _imageService.EncodeImageAsync(Input.Image);
            if (Input.Image != null)
            {
                if (_imageService.IsValidType(Input.Image))
                {
                    user.ImageData = newImageData;
                    user.ImageType = Input.Image.ContentType;
                    await _userManager.UpdateAsync(user);
                    // await _context.SaveChangesAsync();
                }
                else
                {
                    await LoadAsync(user);
                    ModelState.AddModelError("Input.Image", "Please choose an actual image type...");
                    return Page();
                }
            }

            await _signInManager.RefreshSignInAsync(user);
            StatusMessage = "Your profile has been updated";
            return RedirectToPage();
        }
    }
}
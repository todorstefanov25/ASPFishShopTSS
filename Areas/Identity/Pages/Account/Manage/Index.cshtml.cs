using System.ComponentModel.DataAnnotations;
using FishShopASP.Data;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace FishShopASP.Areas.Identity.Pages.Account.Manage
{
    [Authorize]
    public class IndexModel : PageModel
    {
        private readonly UserManager<Client> _userManager;
        private readonly SignInManager<Client> _signInManager;

        public IndexModel(UserManager<Client> userManager, SignInManager<Client> signInManager)
        {
            _userManager = userManager;
            _signInManager = signInManager;
        }

        public string Username { get; set; } = string.Empty;

        [TempData]
        public string? StatusMessage { get; set; }

        [BindProperty]
        public InputModel Input { get; set; } = new();

        public class InputModel
        {
            [Required(ErrorMessage = "Въведете име.")]
            public string FirstName { get; set; } = string.Empty;

            [Required(ErrorMessage = "Въведете фамилия.")]
            public string LastName { get; set; } = string.Empty;

            [Phone(ErrorMessage = "Въведете валиден телефон.")]
            public string? PhoneNumber { get; set; }

            [Required(ErrorMessage = "Въведете имейл.")]
            [EmailAddress(ErrorMessage = "Въведете валиден имейл.")]
            public string Email { get; set; } = string.Empty;
        }

        public async Task<IActionResult> OnGetAsync()
        {
            var user = await _userManager.GetUserAsync(User);
            if (user == null)
            {
                return NotFound($"Unable to load user with ID '{_userManager.GetUserId(User)}'.");
            }

            LoadUserData(user);
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
                LoadUserData(user);
                return Page();
            }

            user.FirstName = Input.FirstName;
            user.LastName = Input.LastName;

            if (Input.PhoneNumber != user.PhoneNumber)
            {
                var phoneResult = await _userManager.SetPhoneNumberAsync(user, Input.PhoneNumber);
                if (!phoneResult.Succeeded)
                {
                    StatusMessage = "Грешка при запазване на телефона.";
                    return RedirectToPage();
                }
            }

            if (Input.Email != user.Email)
            {
                var emailResult = await _userManager.SetEmailAsync(user, Input.Email);
                if (!emailResult.Succeeded)
                {
                    StatusMessage = "Грешка при запазване на имейла.";
                    return RedirectToPage();
                }
            }

            var updateResult = await _userManager.UpdateAsync(user);
            if (!updateResult.Succeeded)
            {
                StatusMessage = "Грешка при запазване на профила.";
                return RedirectToPage();
            }

            await _signInManager.RefreshSignInAsync(user);
            StatusMessage = "Профилът е обновен успешно.";
            return RedirectToPage();
        }

        private void LoadUserData(Client user)
        {
            Username = user.UserName ?? string.Empty;
            Input = new InputModel
            {
                FirstName = user.FirstName,
                LastName = user.LastName,
                PhoneNumber = user.PhoneNumber,
                Email = user.Email ?? string.Empty
            };
        }
    }
}

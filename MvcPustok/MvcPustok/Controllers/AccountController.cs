using System;
using System.Security.Claims;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using MvcPustok.Models;
using MvcPustok.ViewModels;

namespace MvcPustok.Controllers
{
	public class AccountController:Controller
	{
        private readonly UserManager<AppUser> _userManager;
        private readonly SignInManager<AppUser> _signInManager;

        public AccountController(UserManager<AppUser> userManager,SignInManager<AppUser> signInManager)
		{
            _userManager = userManager;
            _signInManager = signInManager;
        }
        public IActionResult Register()
        {
            return View();
        }
        [HttpPost]
        public async Task<IActionResult> Register(MemberRegisterViewModel member)
        {
            if (!ModelState.IsValid)
            {
                return View();
            }
            if (_userManager.Users.Any(x => x.NormalizedEmail == member.Email.ToUpper()))
            {
                ModelState.AddModelError("Email", "Email is already taken");
                return View();
            }
            AppUser appUser = new AppUser()
            {
                UserName = member.UserName,
                Email = member.Email,
                FullName = member.FullName
            };
            var result = await _userManager.CreateAsync(appUser, member.Password);
            if (!result.Succeeded)
            {
                foreach (var item in result.Errors)
                {
                    if (item.Code == "DuplicateUserName")
                        ModelState.AddModelError("UserName", "UserName is already taken");
                    else
                    ModelState.AddModelError("", item.Description);

                }
                return View();
            }
            await _userManager.AddToRoleAsync(appUser, "member");
            return RedirectToAction("login", "account");
        }
        public IActionResult Login()
        {
            return View();
        }
        [HttpPost]
        public async Task<IActionResult> Login(MemberLoginViewModel member, string returnUrl)
        {
          
;            AppUser appUser = await _userManager.FindByEmailAsync(member.Email);

            if (appUser == null || !await _userManager.IsInRoleAsync(appUser,"member"))
            {
                ModelState.AddModelError("", "Email Or Password is not true");
                return View();
            }
            var result = await _signInManager.PasswordSignInAsync(appUser, member.Password, false, true);

            if (!result.Succeeded)
            {
                ModelState.AddModelError("", "Email or Password is incorrect");
                return View();
            }
            return returnUrl != null ? Redirect(returnUrl) : RedirectToAction("index", "home");
        }
        [Authorize(Roles ="member")]
        public async Task<IActionResult> Logout()
        {
            await _signInManager.SignOutAsync();
            return RedirectToAction("index", "home");
        }
        [Authorize(Roles = "member")]
        public async Task<IActionResult> Profile(string tab="dashboard")
        {
            if (!User.Identity.IsAuthenticated)
            {
                return RedirectToAction("login", "account");
            }
            AppUser? user = await _userManager.GetUserAsync(User);
            if (user == null)
            {
                return RedirectToAction("login", "account");
            }
            ProfileViewModel profileViewModel = new ProfileViewModel()
            {
                ProfileEditView = new ProfileEditViewModel()
                {
                    FullName=user.FullName,
                    UserName=user.UserName,
                    Email=user.Email
                }

            };
            ViewBag.Tab = tab;
            return View(profileViewModel);
        }

        [Authorize(Roles = "member")]
        [HttpPost]
        public async Task<IActionResult> Profile(ProfileEditViewModel profileEditView,string tab="profile")
        {
            ViewBag.Tab = tab;
            ProfileViewModel profileViewModel = new ProfileViewModel();
            profileViewModel.ProfileEditView = profileEditView;
            if (!ModelState.IsValid) return View(profileViewModel);

            AppUser? user = await _userManager.GetUserAsync(User);

            user.UserName = profileEditView.UserName;
            user.Email = profileEditView.Email;
            user.FullName = profileEditView.FullName;

            if (_userManager.Users.Any(x => x.Id != User.FindFirstValue(ClaimTypes.NameIdentifier) && x.NormalizedEmail == profileEditView.Email.ToUpper()))
            {
                ModelState.AddModelError("Email", "Email is already taken");
                return View(profileViewModel);
            }

            if (profileEditView.NewPassword != null)
            {
                var passwordResult = await _userManager.ChangePasswordAsync(user, profileEditView.CurrentPassword, profileEditView.NewPassword);

                if (!passwordResult.Succeeded)
                {
                    foreach (var err in passwordResult.Errors)
                        ModelState.AddModelError("", err.Description);

                    return View(profileViewModel);
                }
            }
            var result = await _userManager.UpdateAsync(user);

            if (!result.Succeeded)
            {
                foreach (var err in result.Errors)
                {
                    if (err.Code == "DuplicateUserName")
                        ModelState.AddModelError("UserName", "UserName is already taken");
                  
                    else ModelState.AddModelError("", err.Description);
                }
                return View(profileViewModel);
            }
            await _signInManager.SignInAsync(user,false);
            return View(profileViewModel);

        }

    }
}



using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNet.Identity;
using Microsoft.AspNet.Mvc;
using TheWorld.Models;
using TheWorld.ViewModels;
using System.Security.Claims;
using Microsoft.AspNet.Http;
using TheWorld.Services;

// For more information on enabling MVC for empty projects, visit http://go.microsoft.com/fwlink/?LinkID=397860

namespace TheWorld.Controllers
{
    public class AuthController : Controller
    {
        private SignInManager<WorldUser> _signInManager;
        private UserManager<WorldUser> _userManager;
        private IProfileService _profileService;
        // GET: /<controller>/

        public AuthController(SignInManager<WorldUser> signInManager, UserManager<WorldUser> userManager, IProfileService profileService)
        {
            _signInManager = signInManager;
            _userManager = userManager;
            _profileService = profileService;

        }
        public IActionResult Login()
        {
            if (User.Identity.IsAuthenticated)
            {
                return RedirectToAction("Trips", "App");
            }
            return View();
        }

        [HttpPost]
        public async Task<IActionResult> Login(LoginViewModel vm, string returnUrl)
        {
            if (ModelState.IsValid)
            {


                var signInResult = await _signInManager.PasswordSignInAsync(vm.Username, vm.Password, true, false);
                if (signInResult.Succeeded)
                {
                    //var user = await _userManager.FindByNameAsync(vm.Username);
                    //Context.Session.SetString("useremail", user.Email);

                    //var userImage = await _profileService.GetBase64UserProfileImage(user.Email);
                    //Context.Session.SetString("userimage", $"data:image/gif;base64,{userImage}");
                    
                    if (string.IsNullOrWhiteSpace(returnUrl))
                    {
                        return RedirectToAction("Trips", "App");
                    }
                    else
                    {
                        return Redirect(returnUrl);
                    }

                }
                else
                {
                    ModelState.AddModelError("", "UserName or Password incorrect");
                }
            }
            return View();
        }

        public async Task<IActionResult> Logout()
        {
            if (User.Identity.IsAuthenticated)
            {
                await _signInManager.SignOutAsync();
            }

            return RedirectToAction("Index", "App");
        }
    }
}

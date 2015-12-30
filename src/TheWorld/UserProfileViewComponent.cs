using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNet.Identity;
using Microsoft.AspNet.Mvc;
using TheWorld.Models;
using TheWorld.Services;
using TheWorld.ViewModels;

namespace TheWorld
{
    [ViewComponent(Name = "UserProfileView")]
    public class UserProfileViewComponent : ViewComponent
    {
        private IProfileService _profileService;
        private UserManager<WorldUser> _userManager;

        public UserProfileViewComponent(UserManager<WorldUser> userManager, IProfileService profileService)
        {
            _userManager = userManager;
            _profileService = profileService;
        }

        public async Task<IViewComponentResult> InvokeAsync()
        {
            if (User.Identity.IsAuthenticated)
            {
                var user = await _userManager.FindByNameAsync(User.Identity.Name);
                var image = await _profileService.GetBase64UserProfileImage(user.Email);

                var userProfileVm = new UserProfileViewModel()
                {
                    UserName = user.UserName,
                    Base64Image = image
                };

                return View(userProfileVm);
            }
            return null;
        }
    }
}

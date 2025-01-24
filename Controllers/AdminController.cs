using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using TMCP.Models.Data;
using TMCP.Models.ViewModel;

namespace TMCP.Controllers
{
    public class AdminController : Controller
    {
        private readonly UserManager<User> _userManager;
        private readonly RoleManager<IdentityRole> _roleManager;

        public AdminController(UserManager<User> userManager, RoleManager<IdentityRole> roleManager)
        {
            _userManager = userManager;
            _roleManager = roleManager;
        }

        // GET: Admin/Index
        public async Task<IActionResult> Index()
        {
            var users = _userManager.Users.ToList();
            var userRoleViewModel = new List<UserRoleViewModel>();

            foreach(var user in users)
            {
                var roles = await _userManager.GetRolesAsync(user);
                userRoleViewModel.Add(new UserRoleViewModel
                {
                    UserId = user.Id,
                    Email = user.Email,
                    Roles = roles.ToList()
                });
            }
            return View(userRoleViewModel);
        }

        // GET: Admin/ManageRoles/(userId)
        public async Task<IActionResult> ManageRoles(string userId)
        {
            var user = await _userManager.FindByIdAsync(userId);
            if (user == null)
            {
                return NotFound();
            }

            var roles = _roleManager.Roles.ToList();
            var userRoles = await _userManager.GetRolesAsync(user);

            var model = new ManageRolesViewModel
            {
                UserId = user.Id,
                Email = user.Email,
                Roles = roles.Select(r => new RoleViewModel
                {
                    RoleId = r.Id,
                    RoleName = r.Name,
                    IsSelected = userRoles.Contains(r.Name)
                }).ToList()
            };

            return View(model);
        }

        // POST: Admin/ManageRoles/{userId}
        [HttpPost]
        public async Task<IActionResult> ManageRoles(ManageRolesViewModel model)
        {
            var user = await _userManager.FindByIdAsync(model.UserId);
            if (user == null)
            {
                return NotFound();
            }

            var roles = await _userManager.GetRolesAsync(user);
            var result = await _userManager.RemoveFromRolesAsync(user, roles);

            if (!result.Succeeded)
            {
                ModelState.AddModelError("", "Failde to remove the existing roles. ");
                return View(model);
            }

            result = await _userManager.AddToRolesAsync(user, model.Roles.Where(r => r.IsSelected)
                .Select(r => r.RoleName));

            if (!result.Succeeded)
            {
                ModelState.AddModelError("", "Failde to add new roles. ");
                return View(model);
            }
            return RedirectToAction("Index");
        }
    }
}

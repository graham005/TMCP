using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
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
                }).ToList(),
                SelectedRoleId = userRoles.FirstOrDefault()

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

            // remove all existing roles
            var roles = await _userManager.GetRolesAsync(user);
            var result = await _userManager.RemoveFromRolesAsync(user, roles);

            if (!result.Succeeded)
            {
                ModelState.AddModelError("", "Failed to remove the existing roles. ");
                return View(model);
            }

            // add the selected role
            var selectedRole = await _roleManager.FindByIdAsync(model.SelectedRoleId);
            if (selectedRole != null)
            {
                result = await _userManager.AddToRoleAsync(user, selectedRole.Name);

                if (!result.Succeeded)
                {
                    ModelState.AddModelError("", "Failed to add new roles. ");
                    return View(model);
                }
            }
            return RedirectToAction("Index");
        }

        // GET: Admin/ListUsers
        public async Task<IActionResult> ListUsers()
        {
            var users =  await _userManager.Users.ToListAsync();
            var userViewModels = users.Select(u => new UserViewModel
            {
                Id = u.Id,
                Email = u.Email,
                UserName = u.UserName,
                Roles = _userManager.GetRolesAsync(u).Result.ToList()
            }).ToList();

            return View(userViewModels);
        }

        // GET: Admin/CreateUser
        public IActionResult CreateUser()
        {
            return View();
        }

        // POST: Admin/CreateUser
        [HttpPost]
        public async Task<IActionResult> CreateUser(CreateUserViewModel model)
        {
            if (ModelState.IsValid)
            {
                var user = new User
                {
                    UserName = model.UserName,
                    Email = model.Email,
                };

                var result = await _userManager.CreateAsync(user, model.Password);

                if (result.Succeeded)
                {
                    // Assigned role to the Member role
                    await _userManager.AddToRoleAsync(user, "Member");

                    TempData["Message"] = "User created successfully";
                    return RedirectToAction("ListUsers");
                }
                else
                {
                    TempData["Error"] = "Failed to create user";
                }

                foreach (var error in  result.Errors)
                {
                    ModelState.AddModelError("", error.Description);
                }
            }

            return View(model);
        }

        // GET: Admin/EditUser/{userId}
        [HttpGet]
        public async Task<IActionResult> EditUser(string userId)
        {

            if (string.IsNullOrEmpty(userId))
            {
                return NotFound();
            }
            var user = await _userManager.FindByIdAsync(userId);
            if (user == null)
            {
                return NotFound();  
            }

            var model = new EditUserViewModel
            {
                Id = user.Id,
                Email = user.Email,
                UserName = user.UserName
            };

            return View(model);
        }

        // POST: Admin/EditUser/{userId}
        [HttpPost]
        public async Task<IActionResult> EditUser(EditUserViewModel model)
        {
            if (ModelState.IsValid)
            {
                var user = await _userManager.FindByIdAsync(model.Id);
                if (user == null)
                {
                    return NotFound();
                }

                user.Email = model.Email;
                user.UserName = model.UserName;

                var result = await _userManager.UpdateAsync(user);
                if (result.Succeeded)
                {
                    return RedirectToAction("ListUsers");
                }

                foreach(var error in result.Errors)
                {
                    ModelState.AddModelError("", error.Description);
                }
            }

            return View(model);
        }

        // POST: Admin/DeleteUser/{userId}
        [HttpPost]
        public async Task<IActionResult> DeleteUser(string userId)
        {
            var user = await _userManager.FindByIdAsync(userId);
            if (user == null)
            {
                return NotFound(); 
            }

            var result = await _userManager.DeleteAsync(user);
            if (result.Succeeded)
            {
                TempData["Message"] = "User deleted successfully";
                return RedirectToAction("ListUsers");
            }
            else
            {
                TempData["Message"] = "Error deleting user";
            }

            foreach(var error in result.Errors)
            {
                ModelState.AddModelError("", error.Description);
            }

            return View("ListUsers");
        }
    }
}

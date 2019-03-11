using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using TangyTomato.Data;
using TangyTomato.Models;
using TangyTomato.Models.UsersViewModels;
using TangyTomato.Utility;

namespace TangyTomato.Controllers
{
    public class UsersController : Controller
    {
        private readonly ApplicationDbContext _context;
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly RoleManager<IdentityRole> _roleManager;

        [BindProperty]
        public UserViewModel UserVM { get; set; }

        public UsersController(ApplicationDbContext context, RoleManager<IdentityRole> roleManager, 
            UserManager<ApplicationUser> userManager)
        {
            _context = context;
            _roleManager = roleManager;
            _userManager = userManager;

            UserVM = new UserViewModel()
            {
                ApplicationUser = new Models.ApplicationUser(),
                LockoutEndString = ""
            };
        }

        [HttpGet]
        public async Task<IActionResult> Index()
        {
            var userInRoles = await GetUserInRolesUsingUserManager(_userManager, SD.CustomerEndUser);
            return View(userInRoles);
        }

        /**
         * This is general common method used to get the users having given role with the help 
         * of role manager object in identity framework and "ApplicationDbContext".
         * returns List of Users in terms of "IQueryable"
         * **/
        public async Task<IQueryable<ApplicationUser>> GetUserInRolesUsingRoleManager (ApplicationDbContext applicationDbContext,RoleManager<IdentityRole> roleManager,string roleName)
        {
            var roles = await roleManager.Roles.SingleOrDefaultAsync(x => x.Name == roleName);
            var userRoles = await applicationDbContext.UserRoles.SingleOrDefaultAsync(y => y.RoleId == roles.Id);
            var usersInRoles = applicationDbContext.Users.Where(z => z.Id == userRoles.UserId);
            return usersInRoles;
        }

        /**
        * This is general common method used to get the users having given role with the help 
        * of user manager object in identity framework.
        * returns List of Users in terms of "IList"
        * **/
        public async Task<IList<ApplicationUser>> GetUserInRolesUsingUserManager(UserManager<ApplicationUser> userManager, string roleName)
        {
            var userInRoles = await userManager.GetUsersInRoleAsync(roleName);
            return userInRoles;
        }

        // Edit - Get Method
        public async Task<IActionResult> Edit(string id)
        {
            if(id == null)
            {
                return NotFound();
            }
           //  var userById = await _context.applicationUsers.SingleOrDefaultAsync(u=>u.Id == id.ToString());
            // var user = new ApplicationUser 
            /**
             * If we want to restrict the objects in the table or model then we can proceed 
             * with this otherwise directly we can pass "user" to the "View"
             * **/
            // {
            //     FirstName = userById.FirstName,
            //     LastName = userById.LastName,
            //     Email = userById.Email,
            //      PhoneNumber = userById.PhoneNumber,
            //      LockoutEnd = userById.LockoutEnd,
            //      LockoutReason = userById.LockoutReason,
            //      AccessFailedCount = userById.AccessFailedCount,
            //   };
            //   if (user == null)
            //   {
            //       return NotFound();
            //   }
            //   return View(user);
            UserVM.ApplicationUser = await _context.applicationUsers.SingleOrDefaultAsync(u => u.Id == id.ToString());
            if (UserVM.ApplicationUser.LockoutEnd != null)
            {
                UserVM.LockoutEndString = UserVM.ApplicationUser.LockoutEnd.Value.ToString("MM/dd/yyyy");
            }
            if (UserVM.ApplicationUser == null)
            {
                return NotFound();
            }

            return View(UserVM);

        }

        // Edit - Post Method
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(string id, ApplicationUser applicationUser)
        {
            if (id != applicationUser.Id)
            {
                return NotFound();
            }
            if (UserVM.LockoutEndString != null)
            {
                applicationUser.LockoutEnd = DateTime.Parse(UserVM.LockoutEndString);
            }
            var applicationUserFromDb = await _context.applicationUsers.Where(c => c.Id == id).FirstOrDefaultAsync();

            if (ModelState.IsValid)
            {
                applicationUserFromDb.FirstName = applicationUser.FirstName;
                applicationUserFromDb.LastName = applicationUser.LastName;
                applicationUserFromDb.PhoneNumber = applicationUser.PhoneNumber;
                applicationUserFromDb.LockoutEnd = applicationUser.LockoutEnd;
                applicationUserFromDb.LockoutReason = applicationUser.LockoutReason;
                applicationUserFromDb.AccessFailedCount = applicationUser.AccessFailedCount;

                await _context.SaveChangesAsync();
                return RedirectToAction(nameof(Index));
            }

            return View(applicationUser);
        }

        // lock - Get Method
        public async Task<IActionResult> Lock(string id)
        {
            if (id == null)
            {
                return NotFound();
            }
            UserVM.ApplicationUser = await _context.applicationUsers.SingleOrDefaultAsync(u => u.Id == id.ToString());


            /**
            * If we want to restrict the objects in the table or model then we can proceed 
            * with this otherwise directly we can pass "user" to the "View"
            * **/
            if (UserVM.ApplicationUser.LockoutEnd != null)
            {
                UserVM.LockoutEndString = UserVM.ApplicationUser.LockoutEnd.Value.ToString("MM/dd/yyyy");
            }
            if (UserVM.ApplicationUser == null)
            {
                return NotFound();
            }

            return View(UserVM);
        }


        // Lock - Post Method
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Lock(string id, ApplicationUser applicationUser)
        {
            if (id != applicationUser.Id)
            {
                return NotFound();
            }
           
            var applicationUserFromDb = await _context.applicationUsers.Where(c => c.Id == id).FirstOrDefaultAsync();

            if (ModelState.IsValid)
            {
                applicationUserFromDb.LockoutEnd = applicationUserFromDb.LockoutEnd.Value.AddYears(SD.LockYear);
                applicationUserFromDb.LockoutReason = applicationUser.LockoutReason;

                await _context.SaveChangesAsync();
                return RedirectToAction(nameof(Index));
            }

            return View(applicationUser);
        }
    }
}
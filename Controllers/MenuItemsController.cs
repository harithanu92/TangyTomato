using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using TangyTomato.Data;
using TangyTomato.Models;
using TangyTomato.Models.MenuItemViewModels;
using TangyTomato.Utility;

namespace TangyTomato.Controllers
{
    public class MenuItemsController : Controller
    {
        private readonly ApplicationDbContext _context;
        private readonly IHostingEnvironment _hostingEnvironment;

        [BindProperty]
        public MenuItemViewModel MenuItemVM { get; set; }

        public MenuItemsController (ApplicationDbContext context, IHostingEnvironment hostingEnvironment)
        {
            _context = context;
            _hostingEnvironment = hostingEnvironment; // Used to know the path of server in order to save the images (while save and retireve images)
            MenuItemVM = new MenuItemViewModel()
            {
                Category = _context.Category.ToList(),
                MenuItem = new Models.MenuItem()
            };
        }

        // Get : MenuItems
        public async Task<IActionResult> Index()
        {
            var menuItems = _context.MenuItem.Include(m => m.Category).Include(n => n.SubCategory);
            return View(await menuItems.ToListAsync());
        }

        // Get : Menu Items Create
        public IActionResult Create()
        {
            return View(MenuItemVM);
        }

        // Post : Menu Items Create
        [HttpPost, ActionName("Create")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> CreatePOST()
        /**
          Here no model object passed as input becuase on top of this class we
        used "Bind Property" for MenuItemViewModel so through out this controller 
        it automatically binds the values to the model object "MenuItemVM"
        */
        {
            MenuItemVM.MenuItem.SubCategoryId = Convert.ToInt32(Request.Form["MenuItem.SubCategoryId"].ToString());
            /**
             * Since we used bind property already current records will be available in bind property object 
             * But in create view we are populating "SubCategoryId" alone from java script based on 
             * category id selection(cascading implementation). So that will not be available or it will not
             * handled in bind property so we need to do that manually. Need to get value from Request.form using "ID"
             */
            if (!ModelState.IsValid)
            {
                return View(MenuItemVM);
            }
            _context.MenuItem.Add(MenuItemVM.MenuItem);
            await _context.SaveChangesAsync();

            // Image Being Saved
            string webRootPath = _hostingEnvironment.WebRootPath;
            var files = HttpContext.Request.Form.Files;

            var menuItemFromDb = _context.MenuItem.Find(MenuItemVM.MenuItem.Id);

            if (files[0] != null && files[0].Length > 0)
            {
                // When user uploads an image
                var uploads = Path.Combine(webRootPath + @"\images\");
                var extension = files[0].FileName.Substring
                    (files[0].FileName.LastIndexOf("."), files[0].FileName.Length - files[0].FileName.LastIndexOf("."));

                using (var fileStream = new FileStream(Path.Combine(uploads, MenuItemVM.MenuItem.Id + extension), FileMode.Create))
                {
                    files[0].CopyTo(fileStream);
                }

                menuItemFromDb.Image = @"\images\" + MenuItemVM.MenuItem.Id + extension;
            }
            else
            {
                // When user does not upload image
                var uploads = Path.Combine(webRootPath + @"\images\" + SD.DefaultFoodImage);
                System.IO.File.Copy(uploads, webRootPath + @"\images\" + MenuItemVM.MenuItem.Id + ".png");
                menuItemFromDb.Image = @"\images\" + MenuItemVM.MenuItem.Id + ".png";
            }

            await _context.SaveChangesAsync();
            return RedirectToAction(nameof(Index));
        }

        public JsonResult GetSubCategory (int CategoryId)
        {
            List<SubCategory> subCategoryList = new List<SubCategory>();

            subCategoryList = (from subCategory in _context.SubCategory
                               where subCategory.CategoryId == CategoryId
                               select subCategory).ToList();
            return Json(new SelectList(subCategoryList, "Id","Name"));
        }

        // Get : Edit Menu Items
        public async Task<IActionResult> Edit(int? id)
        {
            if(id == null)
            {
                return NotFound();
            }
            MenuItemVM.MenuItem = await _context.MenuItem.Include(m => m.Category).Include(m => m.SubCategory).SingleOrDefaultAsync(m => m.Id == id);
            MenuItemVM.SubCategory = _context.SubCategory.Where(s => s.CategoryId == MenuItemVM.MenuItem.CategoryId).ToList();

            if(MenuItemVM.MenuItem == null)
            {
                return NotFound();
            }

            return View(MenuItemVM);
        }


        // POST Edit MenuItems
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id)
        {
            MenuItemVM.MenuItem.SubCategoryId = Convert.ToInt32(Request.Form["MenuItem.SubCategoryId"].ToString());
            /**
             * Since we used bind property already current records will be available in bind property object 
             * But in create view we are populating "SubCategoryId" alone from java script based on 
             * category id selection(cascading implementation). So that will not be available or it will not
             * handled in bind property so we need to do that manually. Need to get value from Request.form using "ID"
             */

            if (id != MenuItemVM.MenuItem.Id)
            {
                return NotFound();
            }
            if(ModelState.IsValid)
            {
                try
                {
                    string webRootPath = _hostingEnvironment.WebRootPath;
                    var files = HttpContext.Request.Form.Files;
                    var menuItemsFromDb = _context.MenuItem.Where(m => m.Id == MenuItemVM.MenuItem.Id).FirstOrDefault();

                    if (files[0].Length > 0 && files[0] != null)
                    {
                        // if user uploads new image
                        var uploads = Path.Combine(webRootPath, "images");

                        var extension_New = files[0].FileName.Substring(files[0].FileName.LastIndexOf("."), files[0].FileName.Length - files[0].FileName.LastIndexOf("."));

                        var extension_Old = menuItemsFromDb.Image.Substring(menuItemsFromDb.Image.LastIndexOf("."), menuItemsFromDb.Image.Length - menuItemsFromDb.Image.LastIndexOf("."));

                        if (System.IO.File.Exists(Path.Combine(uploads, MenuItemVM.MenuItem.Id + extension_Old)))
                        {
                            System.IO.File.Delete(Path.Combine(uploads, MenuItemVM.MenuItem.Id + extension_Old));
                        }

                        using (var fileStream = new FileStream(Path.Combine(uploads, MenuItemVM.MenuItem.Id + extension_New), FileMode.Create))
                        {
                            files[0].CopyTo(fileStream);
                        }

                        MenuItemVM.MenuItem.Image = @"\images\" + MenuItemVM.MenuItem.Id + extension_New;
                    }

                    if (MenuItemVM.MenuItem.Image != null)
                    {
                        menuItemsFromDb.Image = MenuItemVM.MenuItem.Image;
                    }
                    menuItemsFromDb.Name = MenuItemVM.MenuItem.Name;
                    menuItemsFromDb.Description = MenuItemVM.MenuItem.Description;
                    menuItemsFromDb.Price = MenuItemVM.MenuItem.Price;
                    menuItemsFromDb.Spicyness = MenuItemVM.MenuItem.Spicyness;
                    menuItemsFromDb.CategoryId = MenuItemVM.MenuItem.CategoryId;
                    menuItemsFromDb.SubCategoryId = MenuItemVM.MenuItem.SubCategoryId;

                    await _context.SaveChangesAsync();
                }
                catch (Exception ex)
                {
                    
                }
                return RedirectToAction(nameof(Index));
            }
            MenuItemVM.SubCategory = _context.SubCategory.Where(s => s.CategoryId == MenuItemVM.MenuItem.CategoryId).ToList();
            return View(MenuItemVM); // these two lines added since in error scenario if the above updates not happen then sub category
            // drop tries to load data if above code not available then it will through null exception in view screen.
        }

        // Get : Details Menu Items
        public async Task<IActionResult> Details(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }
            MenuItemVM.MenuItem = await _context.MenuItem.Include(m => m.Category).Include(m => m.SubCategory).SingleOrDefaultAsync(m => m.Id == id);

            if (MenuItemVM.MenuItem == null)
            {
                return NotFound();
            }

            return View(MenuItemVM);
        }

        // Get : Delete Menu Items
        public async Task<IActionResult> Delete(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }
            MenuItemVM.MenuItem = await _context.MenuItem.Include(m => m.Category).Include(m => m.SubCategory).SingleOrDefaultAsync(m => m.Id == id);

            if (MenuItemVM.MenuItem == null)
            {
                return NotFound();
            }

            return View(MenuItemVM);
        }

        // POST Delete MenuItem
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int? id)
        {
            string webRootPath = _hostingEnvironment.WebRootPath;
            MenuItem menuItem = await _context.MenuItem.FindAsync(id);

            if(menuItem != null)
            {
                var uploads = Path.Combine(webRootPath, "images");
                var extension = menuItem.Image.Substring
                    (menuItem.Image.LastIndexOf("."), menuItem.Image.Length - menuItem.Image.LastIndexOf("."));

                var imagePath = Path.Combine(uploads +"\\"+ menuItem.Id +extension);

                if(System.IO.File.Exists(imagePath))
                {
                    System.IO.File.Delete(imagePath);
                }

                _context.MenuItem.Remove(menuItem);
                await _context.SaveChangesAsync();

            }

            return RedirectToAction(nameof(Index));
        }
    }
}
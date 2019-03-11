using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using TangyTomato.Data;
using TangyTomato.Models;
using TangyTomato.Models.SubCategoryViewModels;

namespace TangyTomato.Controllers
{
    public class SubCategoriesController : Controller
    {
        private readonly ApplicationDbContext _context;

        [TempData]
        public string StatusMessage { get; set; }

        //DOI
        public SubCategoriesController(ApplicationDbContext context)
        {
            _context = context;
        }

        // Get Action
        public async Task<IActionResult> Index()
        {
            var subCategories = _context.SubCategory.Include(s => s.Category);

            return View(await subCategories.ToListAsync());
        }

        // Get Action for Create
        public IActionResult Create()
        {
            SubCategoryAndCategoryViewModel model = new SubCategoryAndCategoryViewModel()
            {
                CategoryList = _context.Category.ToList(),
                SubCategory = new SubCategory(),
                SubCategoryList = _context.SubCategory.OrderBy(p => p.Name).Select(p => p.Name).Distinct().ToList()
            };

            return View(model);
        }

        // POST Create
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(SubCategoryAndCategoryViewModel model)
        {
            if (ModelState.IsValid)
            {
                var doesSubCategoryExists = _context.SubCategory.Where(s => s.Name == model.SubCategory.Name).Count();
                var doesSubCatAndCatExists = _context.SubCategory.Where(s => s.Name == model.SubCategory.Name && s.CategoryId == model.SubCategory.CategoryId).Count();

                if (doesSubCategoryExists > 0 && model.isNew)
                {
                    // Handle Error Message
                    StatusMessage = "Error : Sub Category Name already Exists";
                }
                else
                {
                    if (doesSubCategoryExists == 0 && !model.isNew)
                    {
                        // Handle Error Message
                        StatusMessage = "Error : Sub Category does not Exists";
                    }
                    else
                    {
                        if (doesSubCatAndCatExists > 0)
                        {
                            // handle Error Message
                            StatusMessage = "Error : Category and Sub Category combination Exists";
                        }
                        else
                        {
                            _context.Add(model.SubCategory);
                            await _context.SaveChangesAsync();
                            return RedirectToAction(nameof(Index));
                        }
                    }
                }
            }
            SubCategoryAndCategoryViewModel modelVM = new SubCategoryAndCategoryViewModel()
            {
                CategoryList = _context.Category.ToList(),
                SubCategory = model.SubCategory,
                SubCategoryList = _context.SubCategory.OrderBy(p => p.Name).Select(p => p.Name).ToList(),
                statusMessage = StatusMessage
            };
            return View(modelVM);
        }

        // Get Edit
        public async Task<IActionResult> Edit (int? id)
        {
            if(id == null)
            {
                return NotFound();
            }

            var subCategory = await _context.SubCategory.SingleOrDefaultAsync(m => m.Id == id);
            if(subCategory == null)
            {
                return NotFound();
            }
            SubCategoryAndCategoryViewModel model = new SubCategoryAndCategoryViewModel()
            {
                CategoryList = _context.Category.ToList(),
                SubCategory = subCategory,
                SubCategoryList = _context.SubCategory.Select(p => p.Name).Distinct().ToList()
            };
            return View(model);
        }

        // Edit Post
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, SubCategoryAndCategoryViewModel model)
        {
            if (ModelState.IsValid)
            {
                var doesSubCategoryExists = _context.SubCategory.Where(s => s.Name == model.SubCategory.Name).Count();
                var doesSubCatAndCatExists = _context.SubCategory.Where(s => s.Name == model.SubCategory.Name && s.CategoryId == model.SubCategory.CategoryId).Count();

                if (doesSubCategoryExists == 0)
                {
                    // Handle Error Message
                    StatusMessage = "Error : Sub Category does not Exists. You can not add new sub category here.";
                }
                else
                {
                    if (doesSubCatAndCatExists > 0)
                    {
                        StatusMessage = "Error: Category and Sub Category Combination already Existis.";
                    }
                    else
                    {
                        var subCatFromDb = _context.SubCategory.Find(id);
                        subCatFromDb.Name = model.SubCategory.Name;
                        subCatFromDb.CategoryId = model.SubCategory.CategoryId;
                        await _context.SaveChangesAsync();
                        return RedirectToAction(nameof(Index));
                    }
                }
            }
            SubCategoryAndCategoryViewModel modelVM = new SubCategoryAndCategoryViewModel()
            {
                CategoryList = _context.Category.ToList(),
                SubCategory = model.SubCategory,
                SubCategoryList = _context.SubCategory.Select(p => p.Name).Distinct().ToList(),
                statusMessage = StatusMessage 
            };
            return View(modelVM);
        }

        //Get Details
        public async Task<IActionResult> Details(int? id)
        {
            if(id == null)
            {
                return NotFound();
            }
            var subCategory = await _context.SubCategory.Include(s=>s.Category).SingleOrDefaultAsync(m => m.Id == id);
            if(subCategory == null)
            {
                return NotFound();
            }
            return View(subCategory);
        }

        //Delete
        public async Task<IActionResult> Delete(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }
            var subCategory = await _context.SubCategory.Include(s => s.Category).SingleOrDefaultAsync(m => m.Id == id);
            if (subCategory == null)
            {
                return NotFound();
            }
            return View(subCategory);
        }

        // Post Delete
        [HttpPost,ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int? id)
        {
            if(id == null)
            {
                return NotFound();
            }

            var subCategory = await _context.SubCategory.SingleOrDefaultAsync(m => m.Id == id);
            _context.Remove(subCategory);
            await _context.SaveChangesAsync();
            return RedirectToAction(nameof(Index));
        }
    }
}
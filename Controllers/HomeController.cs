using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using TangyTomato.Data;
using TangyTomato.Models;
using TangyTomato.Models.HomeViewModels;

namespace TangyTomato.Controllers
{
    public class HomeController : Controller
    {
        private readonly ApplicationDbContext _context;

        public HomeController(ApplicationDbContext context)
        {
            _context = context;
        }

        public async Task<IActionResult> Index()
        {
            IndexViewModel indexViewModel = new IndexViewModel() {
                MenuItem = await _context.MenuItem.Include(m => m.Category).Include(m => m.SubCategory).ToListAsync(),
                Category = _context.Category.OrderBy(c =>c.DisplayOrder),
                Coupons = _context.Coupons.Where(c=>c.isActive == true).ToList()
            };
            return View(indexViewModel);
        }

        public IActionResult Error()
        {
            return View(new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
        }
    }
}

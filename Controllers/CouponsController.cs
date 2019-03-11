using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using TangyTomato.Data;
using TangyTomato.Models;

namespace TangyTomato.Controllers
{
    public class CouponsController : Controller
    {
        private readonly ApplicationDbContext _context;

        public CouponsController(ApplicationDbContext context)
        {
            _context = context;
        }

        public async Task<IActionResult> Index()
        {
            return View(await _context.Coupons.ToListAsync());
        }

        // Get Coupons
        public IActionResult Create()
        {
            return View();
        }

        //Post Create Coupons
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(Coupons coupons)
        {
            if(ModelState.IsValid)
            {
                var files = HttpContext.Request.Form.Files;
                if(files[0] != null && files[0].Length >0)
                {
                    byte[] p1 = null;
                    using (var fs1 = files[0].OpenReadStream())
                    {
                        using (var ms1 = new MemoryStream())
                        {
                            fs1.CopyTo(ms1);
                            p1 = ms1.ToArray();
                        }
                    }
                    coupons.Picture = p1;
                    _context.Coupons.Add(coupons);
                    await _context.SaveChangesAsync();
                    return RedirectToAction(nameof(Index));
                }
            }
            return View(coupons);
        }

        // GET Edit Coupons
        public async Task<IActionResult> Edit(int? id)
        {
            if(id==null)
            {
                return NotFound();
            }

            var coupon = await _context.Coupons.SingleOrDefaultAsync(m => m.Id == id);
            if(coupon==null)
            {
                return NotFound();
            }
            return View(coupon);
        }

        // Post Edit Coupons
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, Coupons coupons)
        {
            if(id!= coupons.Id)
            {
                return NotFound();
            }

            var couponFromDb = await _context.Coupons.Where(c => c.Id == id).FirstOrDefaultAsync();

            if (ModelState.IsValid)
            {
                var files = HttpContext.Request.Form.Files;
                if (files[0] != null && files[0].Length > 0)
                {
                    byte[] p1 = null;
                    using (var fs1 = files[0].OpenReadStream())
                    {
                        using (var ms1 = new MemoryStream())
                        {
                            fs1.CopyTo(ms1);
                            p1 = ms1.ToArray();
                        }
                    }
                    couponFromDb.Picture = p1;
                }
                couponFromDb.MinimumAmount = coupons.MinimumAmount;
                couponFromDb.Name = coupons.Name;
                couponFromDb.Discount = coupons.Discount;
                couponFromDb.CouponType = coupons.CouponType;
                couponFromDb.isActive = coupons.isActive;

                await _context.SaveChangesAsync();
                return RedirectToAction(nameof(Index));
            }
            return View(coupons);
        }

        // GET: Coupons/Delete/5 - Delete Coupons
        public async Task<IActionResult> Delete (int? Id)
        {
            if(Id == null)
            {
                return NotFound();
            }

            var coupons = await _context.Coupons.SingleOrDefaultAsync(m => m.Id == Id);

            if(coupons == null)
            {
                return NotFound();
            }

            return View(coupons);
        }

        [HttpPost,ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmend(int? Id)
        {
            var coupons = await _context.Coupons.SingleOrDefaultAsync(m => m.Id == Id);
             _context.Coupons.Remove(coupons);
            await _context.SaveChangesAsync();
            return RedirectToAction(nameof(Index));
        }
    }
}
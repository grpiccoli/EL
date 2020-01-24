using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using ELO.Data;
using ELO.Models;

namespace ELO.Controllers
{
    public class ContinentsController : Controller
    {
        private readonly ApplicationDbContext _context;

        public ContinentsController(ApplicationDbContext context)
        {
            _context = context;
        }

        // GET: Continents
        public async Task<IActionResult> Index()
        {
            return View(await _context.Continents.ToListAsync());
        }

        // GET: Continents/Details/5
        public async Task<IActionResult> Details(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var continent = await _context.Continents
                .SingleOrDefaultAsync(m => m.Id == id);
            if (continent == null)
            {
                return NotFound();
            }

            return View(continent);
        }

        // GET: Continents/Create
        public IActionResult Create()
        {
            return View();
        }

        // POST: Continents/Create
        // To protect from overposting attacks, please enable the specific properties you want to bind to, for 
        // more details see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create([Bind("Id,ISO,Name,Latitude,Longitude")] Continent continent)
        {
            if (ModelState.IsValid)
            {
                _context.Add(continent);
                await _context.SaveChangesAsync();
                return RedirectToAction(nameof(Index));
            }
            return View(continent);
        }

        // GET: Continents/Edit/5
        public async Task<IActionResult> Edit(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var continent = await _context.Continents.SingleOrDefaultAsync(m => m.Id == id);
            if (continent == null)
            {
                return NotFound();
            }
            return View(continent);
        }

        // POST: Continents/Edit/5
        // To protect from overposting attacks, please enable the specific properties you want to bind to, for 
        // more details see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, [Bind("Id,ISO,Name,Latitude,Longitude")] Continent continent)
        {
            if (id != continent.Id)
            {
                return NotFound();
            }

            if (ModelState.IsValid)
            {
                try
                {
                    _context.Update(continent);
                    await _context.SaveChangesAsync();
                }
                catch (DbUpdateConcurrencyException)
                {
                    if (!ContinentExists(continent.Id))
                    {
                        return NotFound();
                    }
                    else
                    {
                        throw;
                    }
                }
                return RedirectToAction(nameof(Index));
            }
            return View(continent);
        }

        // GET: Continents/Delete/5
        public async Task<IActionResult> Delete(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var continent = await _context.Continents
                .SingleOrDefaultAsync(m => m.Id == id);
            if (continent == null)
            {
                return NotFound();
            }

            return View(continent);
        }

        // POST: Continents/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            var continent = await _context.Continents.SingleOrDefaultAsync(m => m.Id == id);
            _context.Continents.Remove(continent);
            await _context.SaveChangesAsync();
            return RedirectToAction(nameof(Index));
        }

        private bool ContinentExists(int id)
        {
            return _context.Continents.Any(e => e.Id == id);
        }
    }
}

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
    public class ComunasController : Controller
    {
        private readonly ApplicationDbContext _context;

        public ComunasController(ApplicationDbContext context)
        {
            _context = context;
        }

        // GET: Comunas
        public async Task<IActionResult> Index()
        {
            var applicationDbContext = _context.Comuna.Include(c => c.Provincia);
            return View(await applicationDbContext.ToListAsync());
        }

        // GET: Comunas/Details/5
        public async Task<IActionResult> Details(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var comuna = await _context.Comuna
                .Include(c => c.Provincia)
                .SingleOrDefaultAsync(m => m.ID == id);
            if (comuna == null)
            {
                return NotFound();
            }

            return View(comuna);
        }

        // GET: Comunas/Create
        public IActionResult Create()
        {
            ViewData["ProvinciaID"] = new SelectList(_context.Provincia, "ID", "ID");
            return View();
        }

        // POST: Comunas/Create
        // To protect from overposting attacks, please enable the specific properties you want to bind to, for 
        // more details see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create([Bind("ID,ProvinciaID,Name,DE,CS")] Comuna comuna)
        {
            if (ModelState.IsValid)
            {
                _context.Add(comuna);
                await _context.SaveChangesAsync();
                return RedirectToAction(nameof(Index));
            }
            ViewData["ProvinciaID"] = new SelectList(_context.Provincia, "ID", "ID", comuna.ProvinciaID);
            return View(comuna);
        }

        // GET: Comunas/Edit/5
        public async Task<IActionResult> Edit(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var comuna = await _context.Comuna.SingleOrDefaultAsync(m => m.ID == id);
            if (comuna == null)
            {
                return NotFound();
            }
            ViewData["ProvinciaID"] = new SelectList(_context.Provincia, "ID", "ID", comuna.ProvinciaID);
            return View(comuna);
        }

        // POST: Comunas/Edit/5
        // To protect from overposting attacks, please enable the specific properties you want to bind to, for 
        // more details see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, [Bind("ID,ProvinciaID,Name,DE,CS")] Comuna comuna)
        {
            if (id != comuna.ID)
            {
                return NotFound();
            }

            if (ModelState.IsValid)
            {
                try
                {
                    _context.Update(comuna);
                    await _context.SaveChangesAsync();
                }
                catch (DbUpdateConcurrencyException)
                {
                    if (!ComunaExists(comuna.ID))
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
            ViewData["ProvinciaID"] = new SelectList(_context.Provincia, "ID", "ID", comuna.ProvinciaID);
            return View(comuna);
        }

        // GET: Comunas/Delete/5
        public async Task<IActionResult> Delete(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var comuna = await _context.Comuna
                .Include(c => c.Provincia)
                .SingleOrDefaultAsync(m => m.ID == id);
            if (comuna == null)
            {
                return NotFound();
            }

            return View(comuna);
        }

        // POST: Comunas/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            var comuna = await _context.Comuna.SingleOrDefaultAsync(m => m.ID == id);
            _context.Comuna.Remove(comuna);
            await _context.SaveChangesAsync();
            return RedirectToAction(nameof(Index));
        }

        private bool ComunaExists(int id)
        {
            return _context.Comuna.Any(e => e.ID == id);
        }
    }
}

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
    public class CoordinatesController : Controller
    {
        private readonly ApplicationDbContext _context;

        public CoordinatesController(ApplicationDbContext context)
        {
            _context = context;
        }

        // GET: Coordinates
        public async Task<IActionResult> Index()
        {
            var applicationDbContext = _context.Coordinate.Include(c => c.Comuna).Include(c => c.Provincia).Include(c => c.Region);
            return View(await applicationDbContext.ToListAsync());
        }

        // GET: Coordinates/Details/5
        public async Task<IActionResult> Details(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var coordinate = await _context.Coordinate
                .Include(c => c.Comuna)
                .Include(c => c.Provincia)
                .Include(c => c.Region)
                .SingleOrDefaultAsync(m => m.ID == id);
            if (coordinate == null)
            {
                return NotFound();
            }

            return View(coordinate);
        }

        // GET: Coordinates/Create
        public IActionResult Create()
        {
            ViewData["ComunaID"] = new SelectList(_context.Set<Comuna>(), "ID", "ID");
            ViewData["ProvinciaID"] = new SelectList(_context.Set<Provincia>(), "ID", "ID");
            ViewData["RegionID"] = new SelectList(_context.Set<Region>(), "ID", "ID");
            return View();
        }

        // POST: Coordinates/Create
        // To protect from overposting attacks, please enable the specific properties you want to bind to, for 
        // more details see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create([Bind("ID,ComunaID,ProvinciaID,RegionID,CountryID,Latitude,Longitude,Vertex")] Coordinate coordinate)
        {
            if (ModelState.IsValid)
            {
                _context.Add(coordinate);
                await _context.SaveChangesAsync();
                return RedirectToAction(nameof(Index));
            }
            ViewData["ComunaID"] = new SelectList(_context.Set<Comuna>(), "ID", "ID", coordinate.ComunaID);
            ViewData["ProvinciaID"] = new SelectList(_context.Set<Provincia>(), "ID", "ID", coordinate.ProvinciaID);
            ViewData["RegionID"] = new SelectList(_context.Set<Region>(), "ID", "ID", coordinate.RegionID);
            return View(coordinate);
        }

        // GET: Coordinates/Edit/5
        public async Task<IActionResult> Edit(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var coordinate = await _context.Coordinate.SingleOrDefaultAsync(m => m.ID == id);
            if (coordinate == null)
            {
                return NotFound();
            }
            ViewData["ComunaID"] = new SelectList(_context.Set<Comuna>(), "ID", "ID", coordinate.ComunaID);
            ViewData["ProvinciaID"] = new SelectList(_context.Set<Provincia>(), "ID", "ID", coordinate.ProvinciaID);
            ViewData["RegionID"] = new SelectList(_context.Set<Region>(), "ID", "ID", coordinate.RegionID);
            return View(coordinate);
        }

        // POST: Coordinates/Edit/5
        // To protect from overposting attacks, please enable the specific properties you want to bind to, for 
        // more details see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, [Bind("ID,ComunaID,ProvinciaID,RegionID,CountryID,Latitude,Longitude,Vertex")] Coordinate coordinate)
        {
            if (id != coordinate.ID)
            {
                return NotFound();
            }

            if (ModelState.IsValid)
            {
                try
                {
                    _context.Update(coordinate);
                    await _context.SaveChangesAsync();
                }
                catch (DbUpdateConcurrencyException)
                {
                    if (!CoordinateExists(coordinate.ID))
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
            ViewData["ComunaID"] = new SelectList(_context.Set<Comuna>(), "ID", "ID", coordinate.ComunaID);
            ViewData["ProvinciaID"] = new SelectList(_context.Set<Provincia>(), "ID", "ID", coordinate.ProvinciaID);
            ViewData["RegionID"] = new SelectList(_context.Set<Region>(), "ID", "ID", coordinate.RegionID);
            return View(coordinate);
        }

        // GET: Coordinates/Delete/5
        public async Task<IActionResult> Delete(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var coordinate = await _context.Coordinate
                .Include(c => c.Comuna)
                .Include(c => c.Provincia)
                .Include(c => c.Region)
                .SingleOrDefaultAsync(m => m.ID == id);
            if (coordinate == null)
            {
                return NotFound();
            }

            return View(coordinate);
        }

        // POST: Coordinates/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            var coordinate = await _context.Coordinate.SingleOrDefaultAsync(m => m.ID == id);
            _context.Coordinate.Remove(coordinate);
            await _context.SaveChangesAsync();
            return RedirectToAction(nameof(Index));
        }

        private bool CoordinateExists(int id)
        {
            return _context.Coordinate.Any(e => e.ID == id);
        }
    }
}

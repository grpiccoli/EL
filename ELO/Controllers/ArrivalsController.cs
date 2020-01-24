using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using ELO.Data;
using ELO.Models;
using Microsoft.AspNetCore.Http;

namespace ELO.Controllers
{
    public class ArrivalsController : Controller
    {
        private readonly ApplicationDbContext _context;

        public ArrivalsController(ApplicationDbContext context)
        {
            _context = context;
        }

        // GET: Arrivals
        public async Task<IActionResult> Index()
        {
            var applicationDbContext = _context.Arrivals.Include(a => a.Commune);
            return View(await applicationDbContext.ToListAsync());
        }

        // GET: Arrivals
        public async Task<IActionResult> Map()
        {
            IQueryCollection q = Request.Query;

            var applicationDbContext = _context.Arrivals
                .Include(a => a.Commune)
                .ThenInclude(c => c.Province)
                .ThenInclude(c => c.Region)
                .Where(c => c.Species.Equals(Species.Erizo));

            string temp = "spe";
            if (!string.IsNullOrEmpty(q["q"]))
            {
                temp = q["q"];
                if(temp == "spa")
                {
                    applicationDbContext = _context.Arrivals
                        .Include(a => a.Commune)
                        .ThenInclude(c => c.Province)
                        .ThenInclude(c => c.Region)
                        .Where(c => c.Species.Equals(Species.Almeja));
                }
                else if (temp == "spl")
                {
                    applicationDbContext = _context.Arrivals
                        .Include(a => a.Commune)
                        .ThenInclude(c => c.Province)
                        .ThenInclude(c => c.Region)
                        .Where(c => c.Species.Equals(Species.Luga));
                }
            }
            ViewData["q"] = temp;

            return View(await applicationDbContext.ToListAsync());
        }

        // GET: Arrivals/Details/5
        public async Task<IActionResult> Details(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var arrival = await _context.Arrivals
                .Include(a => a.Commune)
                .SingleOrDefaultAsync(m => m.Id == id);
            if (arrival == null)
            {
                return NotFound();
            }

            return View(arrival);
        }

        // GET: Arrivals/Create
        public IActionResult Create()
        {
            ViewData["CommuneId"] = new SelectList(_context.Set<Commune>(), "Id", "Id");
            return View();
        }

        // POST: Arrivals/Create
        // To protect from overposting attacks, please enable the specific properties you want to bind to, for 
        // more details see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create([Bind("Id,CommuneId,Date,Species,Kg,Caleta")] Arrival arrival)
        {
            if (ModelState.IsValid)
            {
                _context.Add(arrival);
                await _context.SaveChangesAsync();
                return RedirectToAction(nameof(Index));
            }
            ViewData["CommuneId"] = new SelectList(_context.Set<Commune>(), "Id", "Id", arrival.CommuneId);
            return View(arrival);
        }

        // GET: Arrivals/Edit/5
        public async Task<IActionResult> Edit(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var arrival = await _context.Arrivals.SingleOrDefaultAsync(m => m.Id == id);
            if (arrival == null)
            {
                return NotFound();
            }
            ViewData["CommuneId"] = new SelectList(_context.Set<Commune>(), "Id", "Id", arrival.CommuneId);
            return View(arrival);
        }

        // POST: Arrivals/Edit/5
        // To protect from overposting attacks, please enable the specific properties you want to bind to, for 
        // more details see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, [Bind("Id,CommuneId,Date,Species,Kg,Caleta")] Arrival arrival)
        {
            if (id != arrival.Id)
            {
                return NotFound();
            }

            if (ModelState.IsValid)
            {
                try
                {
                    _context.Update(arrival);
                    await _context.SaveChangesAsync();
                }
                catch (DbUpdateConcurrencyException)
                {
                    if (!ArrivalExists(arrival.Id))
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
            ViewData["CommuneId"] = new SelectList(_context.Set<Commune>(), "Id", "Id", arrival.CommuneId);
            return View(arrival);
        }

        // GET: Arrivals/Delete/5
        public async Task<IActionResult> Delete(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var arrival = await _context.Arrivals
                .Include(a => a.Commune)
                .SingleOrDefaultAsync(m => m.Id == id);
            if (arrival == null)
            {
                return NotFound();
            }

            return View(arrival);
        }

        // POST: Arrivals/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            var arrival = await _context.Arrivals.SingleOrDefaultAsync(m => m.Id == id);
            _context.Arrivals.Remove(arrival);
            await _context.SaveChangesAsync();
            return RedirectToAction(nameof(Index));
        }

        private bool ArrivalExists(int id)
        {
            return _context.Arrivals.Any(e => e.Id == id);
        }
    }
}

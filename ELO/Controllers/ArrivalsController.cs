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
            var applicationDbContext = _context.Arrival.Include(a => a.Comuna);
            return View(await applicationDbContext.ToListAsync());
        }

        // GET: Arrivals
        public async Task<IActionResult> Map()
        {
            IQueryCollection q = Request.Query;

            var applicationDbContext = _context.Arrival
                .Include(a => a.Comuna)
                .ThenInclude(c => c.Provincia)
                .ThenInclude(c => c.Region)
                .Where(c => c.Species.Equals(Species.Erizo));

            string temp = "spe";
            if (!String.IsNullOrEmpty(q["q"]))
            {
                temp = q["q"];
                if(temp == "spa")
                {
                    applicationDbContext = _context.Arrival
                        .Include(a => a.Comuna)
                        .ThenInclude(c => c.Provincia)
                        .ThenInclude(c => c.Region)
                        .Where(c => c.Species.Equals(Species.Almeja));
                }
                else if (temp == "spl")
                {
                    applicationDbContext = _context.Arrival
                        .Include(a => a.Comuna)
                        .ThenInclude(c => c.Provincia)
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

            var arrival = await _context.Arrival
                .Include(a => a.Comuna)
                .SingleOrDefaultAsync(m => m.ID == id);
            if (arrival == null)
            {
                return NotFound();
            }

            return View(arrival);
        }

        // GET: Arrivals/Create
        public IActionResult Create()
        {
            ViewData["ComunaID"] = new SelectList(_context.Set<Comuna>(), "ID", "ID");
            return View();
        }

        // POST: Arrivals/Create
        // To protect from overposting attacks, please enable the specific properties you want to bind to, for 
        // more details see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create([Bind("ID,ComunaID,Date,Species,Kg,Caleta")] Arrival arrival)
        {
            if (ModelState.IsValid)
            {
                _context.Add(arrival);
                await _context.SaveChangesAsync();
                return RedirectToAction(nameof(Index));
            }
            ViewData["ComunaID"] = new SelectList(_context.Set<Comuna>(), "ID", "ID", arrival.ComunaID);
            return View(arrival);
        }

        // GET: Arrivals/Edit/5
        public async Task<IActionResult> Edit(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var arrival = await _context.Arrival.SingleOrDefaultAsync(m => m.ID == id);
            if (arrival == null)
            {
                return NotFound();
            }
            ViewData["ComunaID"] = new SelectList(_context.Set<Comuna>(), "ID", "ID", arrival.ComunaID);
            return View(arrival);
        }

        // POST: Arrivals/Edit/5
        // To protect from overposting attacks, please enable the specific properties you want to bind to, for 
        // more details see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, [Bind("ID,ComunaID,Date,Species,Kg,Caleta")] Arrival arrival)
        {
            if (id != arrival.ID)
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
                    if (!ArrivalExists(arrival.ID))
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
            ViewData["ComunaID"] = new SelectList(_context.Set<Comuna>(), "ID", "ID", arrival.ComunaID);
            return View(arrival);
        }

        // GET: Arrivals/Delete/5
        public async Task<IActionResult> Delete(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var arrival = await _context.Arrival
                .Include(a => a.Comuna)
                .SingleOrDefaultAsync(m => m.ID == id);
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
            var arrival = await _context.Arrival.SingleOrDefaultAsync(m => m.ID == id);
            _context.Arrival.Remove(arrival);
            await _context.SaveChangesAsync();
            return RedirectToAction(nameof(Index));
        }

        private bool ArrivalExists(int id)
        {
            return _context.Arrival.Any(e => e.ID == id);
        }
    }
}

using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using ELO.Data;
using ELO.Models;
using Microsoft.AspNetCore.Authorization;

namespace ELO.Controllers
{
    [Authorize]
    public class StationsController : Controller
    {
        private readonly ApplicationDbContext _context;

        public StationsController(ApplicationDbContext context)
        {
            _context = context;
        }

        // GET: Stations
        public async Task<IActionResult> Index()
        {
            var applicationDbContext = _context.Stations.Include(s => s.Region);
            return View(await applicationDbContext.ToListAsync());
        }

        // GET: Arrivals
        [AllowAnonymous]
        public async Task<IActionResult> Map()
        {
            var applicationDbContext = _context.Stations
                .Include(a => a.Region)
                .Include(a => a.Coordinates)
                .Where(c => c.Coordinates.Count() > 0);
            return View(await applicationDbContext.ToListAsync());
        }

        // GET: Stations/Details/5
        public async Task<IActionResult> Details(string id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var station = await _context.Stations
                .Include(s => s.Region)
                .SingleOrDefaultAsync(m => m.Id == id);
            if (station == null)
            {
                return NotFound();
            }

            return View(station);
        }

        // GET: Stations/Create
        public IActionResult Create()
        {
            ViewData["RegionId"] = new SelectList(_context.Regions, "Id", "Id");
            return View();
        }

        // POST: Stations/Create
        // To protect from overposting attacks, please enable the specific properties you want to bind to, for 
        // more details see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create([Bind("Id,RegionId,Area,Name,Latitude,Longitude")] Station station)
        {
            if (ModelState.IsValid)
            {
                _context.Add(station);
                await _context.SaveChangesAsync();
                return RedirectToAction(nameof(Index));
            }
            ViewData["RegionId"] = new SelectList(_context.Regions, "Id", "Id", station.RegionId);
            return View(station);
        }

        // GET: Stations/Edit/5
        public async Task<IActionResult> Edit(string id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var station = await _context.Stations.SingleOrDefaultAsync(m => m.Id == id);
            if (station == null)
            {
                return NotFound();
            }
            ViewData["RegionId"] = new SelectList(_context.Regions, "Id", "Id", station.RegionId);
            return View(station);
        }

        // POST: Stations/Edit/5
        // To protect from overposting attacks, please enable the specific properties you want to bind to, for 
        // more details see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(string id, [Bind("Id,RegionId,Area,Name,Latitude,Longitude")] Station station)
        {
            if (id != station.Id)
            {
                return NotFound();
            }

            if (ModelState.IsValid)
            {
                try
                {
                    _context.Update(station);
                    await _context.SaveChangesAsync();
                }
                catch (DbUpdateConcurrencyException)
                {
                    if (!StationExists(station.Id))
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
            ViewData["RegionId"] = new SelectList(_context.Regions, "Id", "Id", station.RegionId);
            return View(station);
        }

        // GET: Stations/Delete/5
        public async Task<IActionResult> Delete(string id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var station = await _context.Stations
                .Include(s => s.Region)
                .SingleOrDefaultAsync(m => m.Id == id);
            if (station == null)
            {
                return NotFound();
            }

            return View(station);
        }

        // POST: Stations/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(string id)
        {
            var station = await _context.Stations.SingleOrDefaultAsync(m => m.Id == id);
            _context.Stations.Remove(station);
            await _context.SaveChangesAsync();
            return RedirectToAction(nameof(Index));
        }

        private bool StationExists(string id)
        {
            return _context.Stations.Any(e => e.Id == id);
        }
    }
}

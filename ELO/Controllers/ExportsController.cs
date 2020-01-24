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
    public class ExportsController : Controller
    {
        private readonly ApplicationDbContext _context;

        public ExportsController(ApplicationDbContext context)
        {
            _context = context;
        }

        // GET: Exports
        public async Task<IActionResult> Index()
        {
            var applicationDbContext = _context.Exports
                .Include(e => e.Country)
                .Include(e => e.Region);
            return View(await applicationDbContext.ToListAsync());
        }

        // GET: Exports
        [AllowAnonymous]
        public async Task<IActionResult> Summary()
        {
            var applicationDbContext = _context.Exports;
            return View(await applicationDbContext.ToListAsync());
        }

        // GET: Exports/Details/5
        public async Task<IActionResult> Details(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var export = await _context.Exports
                .Include(e => e.Country)
                .Include(e => e.Region)
                .SingleOrDefaultAsync(m => m.Id == id);
            if (export == null)
            {
                return NotFound();
            }

            return View(export);
        }

        // GET: Exports/Create
        public IActionResult Create()
        {
            ViewData["CountryId"] = new SelectList(_context.Set<Country>(), "Id", "Id");
            ViewData["RegionId"] = new SelectList(_context.Set<Region>(), "Id", "Id");
            return View();
        }

        // POST: Exports/Create
        // To protect from overposting attacks, please enable the specific properties you want to bind to, for 
        // more details see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create([Bind("Id,RegionId,CountryId,Species,Processing,Date,Kg,FOB")] Export export)
        {
            if (ModelState.IsValid)
            {
                _context.Add(export);
                await _context.SaveChangesAsync();
                return RedirectToAction(nameof(Index));
            }
            ViewData["CountryId"] = new SelectList(_context.Set<Country>(), "Id", "Id", export.CountryId);
            ViewData["RegionId"] = new SelectList(_context.Set<Region>(), "Id", "Id", export.RegionId);
            return View(export);
        }

        // GET: Exports/Edit/5
        public async Task<IActionResult> Edit(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var export = await _context.Exports.SingleOrDefaultAsync(m => m.Id == id);
            if (export == null)
            {
                return NotFound();
            }
            ViewData["CountryId"] = new SelectList(_context.Set<Country>(), "Id", "Id", export.CountryId);
            ViewData["RegionId"] = new SelectList(_context.Set<Region>(), "Id", "Id", export.RegionId);
            return View(export);
        }

        // POST: Exports/Edit/5
        // To protect from overposting attacks, please enable the specific properties you want to bind to, for 
        // more details see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, [Bind("Id,RegionId,CountryId,Species,Processing,Date,Kg,FOB")] Export export)
        {
            if (id != export.Id)
            {
                return NotFound();
            }

            if (ModelState.IsValid)
            {
                try
                {
                    _context.Update(export);
                    await _context.SaveChangesAsync();
                }
                catch (DbUpdateConcurrencyException)
                {
                    if (!ExportExists(export.Id))
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
            ViewData["CountryId"] = new SelectList(_context.Set<Country>(), "Id", "Id", export.CountryId);
            ViewData["RegionId"] = new SelectList(_context.Set<Region>(), "Id", "Id", export.RegionId);
            return View(export);
        }

        // GET: Exports/Delete/5
        public async Task<IActionResult> Delete(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var export = await _context.Exports
                .Include(e => e.Country)
                .Include(e => e.Region)
                .SingleOrDefaultAsync(m => m.Id == id);
            if (export == null)
            {
                return NotFound();
            }

            return View(export);
        }

        // POST: Exports/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            var export = await _context.Exports.SingleOrDefaultAsync(m => m.Id == id);
            _context.Exports.Remove(export);
            await _context.SaveChangesAsync();
            return RedirectToAction(nameof(Index));
        }

        private bool ExportExists(int id)
        {
            return _context.Exports.Any(e => e.Id == id);
        }
    }
}

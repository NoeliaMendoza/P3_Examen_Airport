using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using AirportApp.Data;
using AirportApp.Models;

namespace AirportApp.Controllers
{
    public class AirplanesController : Controller
    {
        private readonly AirportContext _context;

        public AirplanesController(AirportContext context)
        {
            _context = context;
        }

        // GET: Airplanes
        public async Task<IActionResult> Index()
        {
            var airportContext = _context.Airplanes.Include(a => a.Type);
            return View(await airportContext.ToListAsync());
        }

        // GET: Airplanes/Details/5
        public async Task<IActionResult> Details(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var airplane = await _context.Airplanes
                .Include(a => a.Type)
                .FirstOrDefaultAsync(m => m.AirplaneId == id);
            if (airplane == null)
            {
                return NotFound();
            }

            return View(airplane);
        }

        // GET: Airplanes/Create
        public IActionResult Create()
        {
            ViewData["TypeId"] = new SelectList(_context.AirplaneTypes, "TypeId", "TypeId");
            return View();
        }

        // POST: Airplanes/Create
        // To protect from overposting attacks, enable the specific properties you want to bind to.
        // For more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create([Bind("AirplaneId,Capacity,TypeId,AirlineId")] Airplane airplane)
        {
            if (ModelState.IsValid)
            {
                _context.Add(airplane);
                await _context.SaveChangesAsync();
                return RedirectToAction(nameof(Index));
            }
            ViewData["TypeId"] = new SelectList(_context.AirplaneTypes, "TypeId", "TypeId", airplane.TypeId);
            return View(airplane);
        }

        // GET: Airplanes/Edit/5
        public async Task<IActionResult> Edit(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var airplane = await _context.Airplanes.FindAsync(id);
            if (airplane == null)
            {
                return NotFound();
            }
            ViewData["TypeId"] = new SelectList(_context.AirplaneTypes, "TypeId", "TypeId", airplane.TypeId);
            return View(airplane);
        }

        // POST: Airplanes/Edit/5
        // To protect from overposting attacks, enable the specific properties you want to bind to.
        // For more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, [Bind("AirplaneId,Capacity,TypeId,AirlineId")] Airplane airplane)
        {
            if (id != airplane.AirplaneId)
            {
                return NotFound();
            }

            if (ModelState.IsValid)
            {
                try
                {
                    _context.Update(airplane);
                    await _context.SaveChangesAsync();
                }
                catch (DbUpdateConcurrencyException)
                {
                    if (!AirplaneExists(airplane.AirplaneId))
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
            ViewData["TypeId"] = new SelectList(_context.AirplaneTypes, "TypeId", "TypeId", airplane.TypeId);
            return View(airplane);
        }

        // GET: Airplanes/Delete/5
        public async Task<IActionResult> Delete(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var airplane = await _context.Airplanes
                .Include(a => a.Type)
                .FirstOrDefaultAsync(m => m.AirplaneId == id);
            if (airplane == null)
            {
                return NotFound();
            }

            return View(airplane);
        }

        // POST: Airplanes/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            var airplane = await _context.Airplanes.FindAsync(id);
            if (airplane != null)
            {
                _context.Airplanes.Remove(airplane);
            }

            await _context.SaveChangesAsync();
            return RedirectToAction(nameof(Index));
        }

        private bool AirplaneExists(int id)
        {
            return _context.Airplanes.Any(e => e.AirplaneId == id);
        }
    }
}

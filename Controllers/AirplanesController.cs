using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using AirportApp.Data;
using AirportApp.Models;

namespace AirportApp.Controllers
{
    [Authorize]
public class AirplanesController : Controller
    {
        private readonly AirportContext _context;

        public AirplanesController(AirportContext context)
        {
            _context = context;
        }

        // GET: Airplanes
        public async Task<IActionResult> Index(string? buscar, string? filtro1, string? filtro2, string? orden, int pagina = 1)
        {
            const int tamanoPagina = 20;

            var consulta = _context.Airplanes
                .AsNoTracking()
                .Include(a => a.Type)
                .AsQueryable();

            if (!string.IsNullOrWhiteSpace(buscar))
                consulta = consulta.Where(a =>
                    a.Type.Identifier != null && a.Type.Identifier.Contains(buscar) ||
                    a.Type.Description != null && a.Type.Description.Contains(buscar));

            if (!string.IsNullOrWhiteSpace(filtro1))
            {
                int airlineId = int.TryParse(filtro1, out var a) ? a : 0;
                if (airlineId > 0)
                    consulta = consulta.Where(x => x.AirlineId == airlineId);
            }

            if (!string.IsNullOrWhiteSpace(filtro2))
            {
                int typeId = int.TryParse(filtro2, out var t) ? t : 0;
                if (typeId > 0)
                    consulta = consulta.Where(x => x.TypeId == typeId);
            }

            consulta = orden switch
            {
                "capacidad" => consulta.OrderByDescending(a => a.Capacity),
                "tipo" => consulta.OrderBy(a => a.Type.Description),
                _ => consulta.OrderBy(a => a.AirplaneId),
            };

            int totalRegistros = await consulta.CountAsync();

            var aeronaves = await consulta
                .Skip((pagina - 1) * tamanoPagina)
                .Take(tamanoPagina)
                .ToListAsync();

            ViewBag.Paginacion = new AirportApp.ViewModels.PaginacionViewModel
            {
                PaginaActual = pagina,
                TotalPaginas = (int)Math.Ceiling((double)totalRegistros / tamanoPagina),
                TotalRegistros = totalRegistros,
                TamanoPagina = tamanoPagina,
                Buscar = buscar,
                Filtro1 = filtro1,
                Filtro2 = filtro2,
                Orden = orden,
            };

            ViewData["Aerolineas"] = new SelectList(
                _context.Airlines.AsNoTracking().OrderBy(a => a.Airlinename),
                "AirlineId", "Airlinename", filtro1);

            ViewData["Tipos"] = new SelectList(
                _context.AirplaneTypes.AsNoTracking().OrderBy(t => t.Identifier),
                "TypeId", "Identifier", filtro2);

            return View(aeronaves);
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
                .Include(a => a.Flights)
                .FirstOrDefaultAsync(m => m.AirplaneId == id);
            if (airplane == null)
            {
                return NotFound();
            }

            ViewData["Aerolinea"] = await _context.Airlines
                .AsNoTracking()
                .FirstOrDefaultAsync(al => al.AirlineId == airplane.AirlineId);

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

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
    [Authorize(Roles = "Administrador")]
public class AirlinesController : Controller
    {
        private readonly AirportContext _context;

        public AirlinesController(AirportContext context)
        {
            _context = context;
        }

        // GET: Airlines
        public async Task<IActionResult> Index(string? buscar, string? filtro1, string? orden, int pagina = 1)
        {
            const int tamanoPagina = 20;

            var consulta = _context.Airlines.AsNoTracking().AsQueryable();

            if (!string.IsNullOrWhiteSpace(buscar))
                consulta = consulta.Where(a =>
                    a.Iata.Contains(buscar) ||
                    a.Airlinename != null && a.Airlinename.Contains(buscar));

            if (!string.IsNullOrWhiteSpace(filtro1))
            {
                int baseAirport = int.TryParse(filtro1, out var b) ? b : 0;
                if (baseAirport > 0)
                    consulta = consulta.Where(a => a.BaseAirport == baseAirport);
            }

            consulta = orden switch
            {
                "nombre" => consulta.OrderBy(a => a.Airlinename),
                "nombre_desc" => consulta.OrderByDescending(a => a.Airlinename),
                "iata" => consulta.OrderBy(a => a.Iata),
                _ => consulta.OrderBy(a => a.AirlineId),
            };

            int totalRegistros = await consulta.CountAsync();

            var aerolineas = await consulta
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
                Orden = orden,
            };

            ViewData["Aeropuertos"] = new SelectList(
                _context.Airports.AsNoTracking().OrderBy(p => p.Name),
                "AirportId", "Name", filtro1);

            return View(aerolineas);
        }

        // GET: Airlines/Details/5
        public async Task<IActionResult> Details(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var airline = await _context.Airlines
                .FirstOrDefaultAsync(m => m.AirlineId == id);
            if (airline == null)
            {
                return NotFound();
            }

            return View(airline);
        }

        // GET: Airlines/Create
        public IActionResult Create()
        {
            return View();
        }

        // POST: Airlines/Create
        // To protect from overposting attacks, enable the specific properties you want to bind to.
        // For more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create([Bind("AirlineId,Iata,Airlinename,BaseAirport")] Airline airline)
        {
            if (ModelState.IsValid)
            {
                _context.Add(airline);
                await _context.SaveChangesAsync();
                return RedirectToAction(nameof(Index));
            }
            return View(airline);
        }

        // GET: Airlines/Edit/5
        public async Task<IActionResult> Edit(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var airline = await _context.Airlines.FindAsync(id);
            if (airline == null)
            {
                return NotFound();
            }
            return View(airline);
        }

        // POST: Airlines/Edit/5
        // To protect from overposting attacks, enable the specific properties you want to bind to.
        // For more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, [Bind("AirlineId,Iata,Airlinename,BaseAirport")] Airline airline)
        {
            if (id != airline.AirlineId)
            {
                return NotFound();
            }

            if (ModelState.IsValid)
            {
                try
                {
                    _context.Update(airline);
                    await _context.SaveChangesAsync();
                }
                catch (DbUpdateConcurrencyException)
                {
                    if (!AirlineExists(airline.AirlineId))
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
            return View(airline);
        }

        // GET: Airlines/Delete/5
        public async Task<IActionResult> Delete(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var airline = await _context.Airlines
                .FirstOrDefaultAsync(m => m.AirlineId == id);
            if (airline == null)
            {
                return NotFound();
            }

            return View(airline);
        }

        // POST: Airlines/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            var airline = await _context.Airlines.FindAsync(id);
            if (airline != null)
            {
                _context.Airlines.Remove(airline);
            }

            await _context.SaveChangesAsync();
            return RedirectToAction(nameof(Index));
        }

        private bool AirlineExists(int id)
        {
            return _context.Airlines.Any(e => e.AirlineId == id);
        }
    }
}

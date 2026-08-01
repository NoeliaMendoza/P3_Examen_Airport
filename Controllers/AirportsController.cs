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
public class AirportsController : Controller
    {
        private readonly AirportContext _context;

        public AirportsController(AirportContext context)
        {
            _context = context;
        }

        // GET: Airports
        public async Task<IActionResult> Index(string? buscar, string? filtro1, string? orden, int pagina = 1)
        {
            const int tamanoPagina = 20;

            var consulta = _context.Airports.AsNoTracking().AsQueryable();

            if (!string.IsNullOrWhiteSpace(buscar))
                consulta = consulta.Where(a =>
                    a.Iata != null && a.Iata.Contains(buscar) ||
                    a.Icao.Contains(buscar) ||
                    a.Name.Contains(buscar));

            if (!string.IsNullOrWhiteSpace(filtro1))
                consulta = consulta.Where(a => a.Icao.Contains(filtro1));

            consulta = orden switch
            {
                "nombre" => consulta.OrderBy(a => a.Name),
                "nombre_desc" => consulta.OrderByDescending(a => a.Name),
                "iata" => consulta.OrderBy(a => a.Iata),
                "icao" => consulta.OrderBy(a => a.Icao),
                _ => consulta.OrderBy(a => a.AirportId),
            };

            int totalRegistros = await consulta.CountAsync();

            var aeropuertos = await consulta
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

            return View(aeropuertos);
        }

        // GET: Airports/Details/5
        public async Task<IActionResult> Details(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var airport = await _context.Airports
                .FirstOrDefaultAsync(m => m.AirportId == id);
            if (airport == null)
            {
                return NotFound();
            }

            return View(airport);
        }

        // GET: Airports/Create
        public IActionResult Create()
        {
            return View();
        }

        // POST: Airports/Create
        // To protect from overposting attacks, enable the specific properties you want to bind to.
        // For more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create([Bind("AirportId,Iata,Icao,Name")] Airport airport)
        {
            if (ModelState.IsValid)
            {
                _context.Add(airport);
                await _context.SaveChangesAsync();
                return RedirectToAction(nameof(Index));
            }
            return View(airport);
        }

        // GET: Airports/Edit/5
        public async Task<IActionResult> Edit(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var airport = await _context.Airports.FindAsync(id);
            if (airport == null)
            {
                return NotFound();
            }
            return View(airport);
        }

        // POST: Airports/Edit/5
        // To protect from overposting attacks, enable the specific properties you want to bind to.
        // For more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, [Bind("AirportId,Iata,Icao,Name")] Airport airport)
        {
            if (id != airport.AirportId)
            {
                return NotFound();
            }

            if (ModelState.IsValid)
            {
                try
                {
                    _context.Update(airport);
                    await _context.SaveChangesAsync();
                }
                catch (DbUpdateConcurrencyException)
                {
                    if (!AirportExists(airport.AirportId))
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
            return View(airport);
        }

        // GET: Airports/Delete/5
        public async Task<IActionResult> Delete(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var airport = await _context.Airports
                .FirstOrDefaultAsync(m => m.AirportId == id);
            if (airport == null)
            {
                return NotFound();
            }

            return View(airport);
        }

        // POST: Airports/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            var airport = await _context.Airports.FindAsync(id);
            if (airport != null)
            {
                _context.Airports.Remove(airport);
            }

            await _context.SaveChangesAsync();
            return RedirectToAction(nameof(Index));
        }

        private bool AirportExists(int id)
        {
            return _context.Airports.Any(e => e.AirportId == id);
        }
    }
}

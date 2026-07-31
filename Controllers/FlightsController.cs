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
    public class FlightsController : Controller
    {
        private readonly AirportContext _context;

        public FlightsController(AirportContext context)
        {
            _context = context;
        }

        // GET: Flights
        public async Task<IActionResult> Index(string? buscar, string? filtro1, string? filtro2, string? orden, int pagina = 1)
        {
            const int tamanoPagina = 20;

            var consulta = _context.Flights
                .AsNoTracking()
                .Include(f => f.Airline)
                .Include(f => f.Airplane)
                .Include(f => f.FromNavigation)
                .Include(f => f.ToNavigation)
                .AsQueryable();

            if (!string.IsNullOrWhiteSpace(buscar))
                consulta = consulta.Where(f =>
                    f.Flightno.Contains(buscar) ||
                    f.FromNavigation.Iata.Contains(buscar) ||
                    f.ToNavigation.Iata.Contains(buscar));

            if (!string.IsNullOrWhiteSpace(filtro1))
            {
                int airlineId = int.TryParse(filtro1, out var a) ? a : 0;
                if (airlineId > 0)
                    consulta = consulta.Where(f => f.AirlineId == airlineId);
            }

            if (!string.IsNullOrWhiteSpace(filtro2))
            {
                if (DateTime.TryParse(filtro2, out var fecha))
                    consulta = consulta.Where(f => f.Departure.Date == fecha.Date);
            }

            consulta = orden switch
            {
                "salida_desc" => consulta.OrderByDescending(f => f.Departure),
                "llegada" => consulta.OrderBy(f => f.Arrival),
                "duracion" => consulta.OrderBy(f => f.Arrival - f.Departure),
                _ => consulta.OrderBy(f => f.Departure),
            };

            int totalRegistros = await consulta.CountAsync();

            var vuelos = await consulta
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

            ViewData["Aerolineas"] = new SelectList(_context.Airlines.AsNoTracking().OrderBy(a => a.Airlinename), "AirlineId", "Airlinename", filtro1);

            return View(vuelos);
        }

        // GET: Flights/Details/5
        public async Task<IActionResult> Details(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var flight = await _context.Flights
                .Include(f => f.Airline)
                .Include(f => f.Airplane)
                .Include(f => f.FlightnoNavigation)
                .Include(f => f.FromNavigation)
                .Include(f => f.ToNavigation)
                .FirstOrDefaultAsync(m => m.FlightId == id);
            if (flight == null)
            {
                return NotFound();
            }

            return View(flight);
        }

        // GET: Flights/Create
        public IActionResult Create()
        {
            ViewData["AirlineId"] = new SelectList(_context.Airlines, "AirlineId", "AirlineId");
            ViewData["AirplaneId"] = new SelectList(_context.Airplanes, "AirplaneId", "AirplaneId");
            ViewData["Flightno"] = new SelectList(_context.Flightschedules, "Flightno", "Flightno");
            ViewData["From"] = new SelectList(_context.Airports, "AirportId", "AirportId");
            ViewData["To"] = new SelectList(_context.Airports, "AirportId", "AirportId");
            return View();
        }

        // POST: Flights/Create
        // To protect from overposting attacks, enable the specific properties you want to bind to.
        // For more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create([Bind("FlightId,Flightno,From,To,Departure,Arrival,AirlineId,AirplaneId")] Flight flight)
        {
            if (ModelState.IsValid)
            {
                _context.Add(flight);
                await _context.SaveChangesAsync();
                return RedirectToAction(nameof(Index));
            }
            ViewData["AirlineId"] = new SelectList(_context.Airlines, "AirlineId", "AirlineId", flight.AirlineId);
            ViewData["AirplaneId"] = new SelectList(_context.Airplanes, "AirplaneId", "AirplaneId", flight.AirplaneId);
            ViewData["Flightno"] = new SelectList(_context.Flightschedules, "Flightno", "Flightno", flight.Flightno);
            ViewData["From"] = new SelectList(_context.Airports, "AirportId", "AirportId", flight.From);
            ViewData["To"] = new SelectList(_context.Airports, "AirportId", "AirportId", flight.To);
            return View(flight);
        }

        // GET: Flights/Edit/5
        public async Task<IActionResult> Edit(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var flight = await _context.Flights.FindAsync(id);
            if (flight == null)
            {
                return NotFound();
            }
            ViewData["AirlineId"] = new SelectList(_context.Airlines, "AirlineId", "AirlineId", flight.AirlineId);
            ViewData["AirplaneId"] = new SelectList(_context.Airplanes, "AirplaneId", "AirplaneId", flight.AirplaneId);
            ViewData["Flightno"] = new SelectList(_context.Flightschedules, "Flightno", "Flightno", flight.Flightno);
            ViewData["From"] = new SelectList(_context.Airports, "AirportId", "AirportId", flight.From);
            ViewData["To"] = new SelectList(_context.Airports, "AirportId", "AirportId", flight.To);
            return View(flight);
        }

        // POST: Flights/Edit/5
        // To protect from overposting attacks, enable the specific properties you want to bind to.
        // For more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, [Bind("FlightId,Flightno,From,To,Departure,Arrival,AirlineId,AirplaneId")] Flight flight)
        {
            if (id != flight.FlightId)
            {
                return NotFound();
            }

            if (ModelState.IsValid)
            {
                try
                {
                    _context.Update(flight);
                    await _context.SaveChangesAsync();
                }
                catch (DbUpdateConcurrencyException)
                {
                    if (!FlightExists(flight.FlightId))
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
            ViewData["AirlineId"] = new SelectList(_context.Airlines, "AirlineId", "AirlineId", flight.AirlineId);
            ViewData["AirplaneId"] = new SelectList(_context.Airplanes, "AirplaneId", "AirplaneId", flight.AirplaneId);
            ViewData["Flightno"] = new SelectList(_context.Flightschedules, "Flightno", "Flightno", flight.Flightno);
            ViewData["From"] = new SelectList(_context.Airports, "AirportId", "AirportId", flight.From);
            ViewData["To"] = new SelectList(_context.Airports, "AirportId", "AirportId", flight.To);
            return View(flight);
        }

        // GET: Flights/Delete/5
        public async Task<IActionResult> Delete(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var flight = await _context.Flights
                .Include(f => f.Airline)
                .Include(f => f.Airplane)
                .Include(f => f.FlightnoNavigation)
                .Include(f => f.FromNavigation)
                .Include(f => f.ToNavigation)
                .FirstOrDefaultAsync(m => m.FlightId == id);
            if (flight == null)
            {
                return NotFound();
            }

            return View(flight);
        }

        // POST: Flights/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            var flight = await _context.Flights.FindAsync(id);
            if (flight != null)
            {
                _context.Flights.Remove(flight);
            }

            await _context.SaveChangesAsync();
            return RedirectToAction(nameof(Index));
        }

        private bool FlightExists(int id)
        {
            return _context.Flights.Any(e => e.FlightId == id);
        }
    }
}

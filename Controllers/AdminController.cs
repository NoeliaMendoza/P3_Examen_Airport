using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using AirportApp.Data;

namespace AirportApp.Controllers;

[Authorize(Roles = "Administrador")]
public class AdminController : Controller
{
    private readonly AirportContext _context;
    private readonly ApplicationDbContext _app;
    private readonly UserManager<IdentityUser> _userManager;

    public AdminController(
        AirportContext context,
        ApplicationDbContext app,
        UserManager<IdentityUser> userManager)
    {
        _context = context;
        _app = app;
        _userManager = userManager;
    }

    public async Task<IActionResult> Index()
    {
        ViewBag.TotalVuelos = await _context.Flights.CountAsync();
        ViewBag.TotalAeropuertos = await _context.Airports.CountAsync();
        ViewBag.TotalAerolineas = await _context.Airlines.CountAsync();
        ViewBag.TotalAeronaves = await _context.Airplanes.CountAsync();
        ViewBag.TotalPasajeros = await _context.Passengers.CountAsync();
        ViewBag.TotalUsuarios = await _userManager.Users.CountAsync();

        var reservasActivas = await _app.SeatReservations
            .CountAsync(r => !r.IsConfirmed && r.ExpiresAt > DateTime.UtcNow);
        ViewBag.ReservasActivas = reservasActivas;

        var asientosOcupados = await _app.Seats.CountAsync(s => s.IsOccupied);
        var asientosTotales = await _app.Seats.CountAsync();
        ViewBag.AsientosOcupados = asientosOcupados;
        ViewBag.AsientosTotales = asientosTotales;

        var ordenes = await _app.Orders.CountAsync();
        var ordenesAprobadas = await _app.Orders.CountAsync(o => o.Status == "Aprobado");
        var ingresos = await _app.Payments
            .Where(p => p.Status == "Aprobado")
            .SumAsync(p => p.Amount);
        ViewBag.OrdenesTotales = ordenes;
        ViewBag.OrdenesAprobadas = ordenesAprobadas;
        ViewBag.Ingresos = ingresos;

        var ventasPorPasarela = await _app.Payments
            .AsNoTracking()
            .Where(p => p.Status == "Aprobado")
            .GroupBy(p => p.Gateway)
            .Select(g => new { Pasarela = g.Key, Total = g.Sum(x => x.Amount), Pagos = g.Count() })
            .OrderByDescending(g => g.Total)
            .ToListAsync();
        ViewBag.VentasPorPasarela = ventasPorPasarela;

        var ventasPorMes = await _app.Payments
            .AsNoTracking()
            .Where(p => p.Status == "Aprobado")
            .GroupBy(p => new { p.CreationDate.Year, p.CreationDate.Month })
            .Select(g => new { g.Key.Year, g.Key.Month, Total = g.Sum(x => x.Amount) })
            .OrderByDescending(g => g.Year).ThenByDescending(g => g.Month)
            .Take(6)
            .ToListAsync();
        ViewBag.VentasPorMes = ventasPorMes;

        var ultimosPagos = await _app.Payments
            .AsNoTracking()
            .OrderByDescending(p => p.CreationDate)
            .Take(10)
            .ToListAsync();
        ViewBag.UltimosPagos = ultimosPagos;

        return View();
    }
}

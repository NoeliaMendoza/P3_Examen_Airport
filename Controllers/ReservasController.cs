using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using AirportApp.Data;
using AirportApp.Models.Commerce;
using AirportApp.Services;
using AirportApp.Services.Payments;
using AirportApp.ViewModels;

namespace AirportApp.Controllers;

[Authorize]
public class ReservasController : Controller
{
    private readonly AirportContext _airport;
    private readonly ApplicationDbContext _app;
    private readonly SeatService _seats;
    private readonly UserManager<IdentityUser> _userManager;
    private readonly PayPalService _payPalService;
    private readonly PayPhoneApiLinkService _payPhoneService;

    public ReservasController(
        AirportContext airport,
        ApplicationDbContext app,
        SeatService seats,
        UserManager<IdentityUser> userManager,
        PayPalService payPalService,
        PayPhoneApiLinkService payPhoneService)
    {
        _airport = airport;
        _app = app;
        _seats = seats;
        _userManager = userManager;
        _payPalService = payPalService;
        _payPhoneService = payPhoneService;
    }

    public async Task<IActionResult> Index()
    {
        var userId = _userManager.GetUserId(User);

        var reservas = await _app.SeatReservations
            .AsNoTracking()
            .Where(r => r.UserId == userId)
            .OrderByDescending(r => r.ReservedAt)
            .Take(20)
            .ToListAsync();

        return View(reservas);
    }

    public async Task<IActionResult> Asientos(int id, string? clase)
    {
        var vuelo = await _seats.GetFlightAsync(id);
        if (vuelo == null)
            return NotFound();

        var asientos = await _seats.GetSeatsAsync(id);
        if (asientos.Count == 0)
            return View("SinAeronave", vuelo);

        var tomados = new HashSet<int>();

        var ocupados = await _app.Seats
            .AsNoTracking()
            .Where(s => s.FlightId == id && s.IsOccupied)
            .Select(s => s.SeatId)
            .ToListAsync();

        var activos = await _app.SeatReservations
            .AsNoTracking()
            .Where(r => !r.IsConfirmed && r.ExpiresAt > DateTime.UtcNow && r.UserId != _userManager.GetUserId(User))
            .Select(r => r.SeatId)
            .ToListAsync();

        tomados.UnionWith(ocupados);
        tomados.UnionWith(activos);

        var modelo = new AsientosViewModel
        {
            Vuelo = vuelo,
            Asientos = asientos,
            Clase = clase,
            Ocupados = tomados.Count,
            Disponibles = asientos.Count - tomados.Count,
            PrecioPromedio = asientos.Count > 0 ? asientos.Average(s => s.Price) : 0,
        };

        ViewBag.Tomados = tomados;
        return View(modelo);
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Reservar(int flightId, string seatNo)
    {
        var seat = await _seats.FindSeatAsync(flightId, seatNo);
        if (seat == null)
            return BadRequest("Asiento inexistente.");

        if (await _seats.SeatIsTakenAsync(flightId, seat.SeatId, _userManager.GetUserId(User)))
            return RedirectToAction(nameof(Asientos), new { id = flightId });

        var userId = _userManager.GetUserId(User)!;

        var activa = await _app.SeatReservations
            .FirstOrDefaultAsync(r => r.UserId == userId && r.FlightId == flightId && !r.IsConfirmed && r.ExpiresAt > DateTime.UtcNow);

        if (activa != null)
            return RedirectToAction(nameof(Checkout), new { reservaId = activa.SeatReservationId });

        var reserva = new SeatReservation
        {
            UserId = userId,
            FlightId = flightId,
            SeatId = seat.SeatId,
            ReservedAt = DateTime.UtcNow,
            ExpiresAt = DateTime.UtcNow.AddMinutes(10),
            IsConfirmed = false,
        };

        _app.SeatReservations.Add(reserva);
        await _app.SaveChangesAsync();

        return RedirectToAction(nameof(Checkout), new { reservaId = reserva.SeatReservationId });
    }

    public async Task<IActionResult> Checkout(int reservaId)
    {
        var userId = _userManager.GetUserId(User);

        var reserva = await _app.SeatReservations
            .AsNoTracking()
            .FirstOrDefaultAsync(r => r.SeatReservationId == reservaId && r.UserId == userId);

        if (reserva == null)
            return NotFound();

        if (reserva.ExpiresAt <= DateTime.UtcNow)
        {
            _app.SeatReservations.Remove(reserva);
            await _app.SaveChangesAsync();
            return View("Expirada");
        }

        var vuelo = await _seats.GetFlightAsync(reserva.FlightId);
        var seat = await _app.Seats.AsNoTracking().FirstOrDefaultAsync(s => s.SeatId == reserva.SeatId);

        if (vuelo == null || seat == null)
            return NotFound();

        ViewBag.Reserva = reserva;
        ViewBag.Vuelo = vuelo;
        ViewBag.Asiento = seat;
        return View();
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> CrearOrden(int reservaId)
    {
        var userId = _userManager.GetUserId(User)!;

        var reserva = await _app.SeatReservations
            .FirstOrDefaultAsync(r => r.SeatReservationId == reservaId && r.UserId == userId);

        if (reserva == null)
            return NotFound();

        if (reserva.ExpiresAt <= DateTime.UtcNow)
        {
            _app.SeatReservations.Remove(reserva);
            await _app.SaveChangesAsync();
            return View("Expirada");
        }

        if (await _seats.SeatIsTakenAsync(reserva.FlightId, reserva.SeatId, userId))
        {
            _app.SeatReservations.Remove(reserva);
            await _app.SaveChangesAsync();
            return RedirectToAction(nameof(Asientos), new { id = reserva.FlightId });
        }

        var seat = await _app.Seats.FirstOrDefaultAsync(s => s.SeatId == reserva.SeatId);
        var vuelo = await _seats.GetFlightAsync(reserva.FlightId);
        if (seat == null || vuelo == null)
            return NotFound();

        var total = seat.Price;

        var orden = new Order
        {
            UserId = userId,
            CreatedAt = DateTime.UtcNow,
            Total = total,
            Status = "Pendiente",
        };

        orden.Details.Add(new OrderDetail
        {
            FlightId = vuelo.FlightId,
            SeatId = seat.SeatId,
            Description = $"Vuelo {vuelo.Flightno} - Asiento {seat.SeatNo} ({seat.SeatClass})",
            Quantity = 1,
            UnitPrice = seat.Price,
            Subtotal = seat.Price,
        });

        _app.Orders.Add(orden);
        await _app.SaveChangesAsync();

        reserva.IsConfirmed = true;
        await _app.SaveChangesAsync();

        return RedirectToAction(nameof(Pago), new { id = orden.OrderId });
    }

    public async Task<IActionResult> Pago(int id)
    {
        var userId = _userManager.GetUserId(User);

        var orden = await _app.Orders
            .AsNoTracking()
            .Include(o => o.Details)
            .FirstOrDefaultAsync(o => o.OrderId == id && o.UserId == userId);

        if (orden == null)
            return NotFound();

        var pagos = await _app.Payments
            .AsNoTracking()
            .Where(p => p.OrderId == id)
            .OrderByDescending(p => p.CreationDate)
            .ToListAsync();

        ViewBag.Pagos = pagos;
        return View(orden);
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> CrearPagoPayPal(int orderId)
    {
        var userId = _userManager.GetUserId(User)!;

        var orden = await _app.Orders
            .Include(o => o.Details)
            .FirstOrDefaultAsync(o => o.OrderId == orderId && o.UserId == userId);

        if (orden == null)
            return NotFound();

        if (orden.Total < 1.00m)
        {
            TempData["Error"] = "El monto mínimo para pagar con PayPal Sandbox es de $1.00.";
            return RedirectToAction(nameof(Pago), new { id = orderId });
        }

        string reference = $"Orden AirportApp #{orden.OrderId}";

        var result = await _payPalService.CreateOrderAsync(orden.Total, reference);

        var payment = new Payment
        {
            OrderId = orden.OrderId,
            UserId = userId,
            Gateway = "PayPal",
            ExternalTransactionId = result.OrderId,
            ApprovalUrl = result.ApprovalUrl,
            Amount = orden.Total,
            Currency = "USD",
            Status = "Pendiente",
            CreationDate = DateTime.UtcNow,
            ResponseMessage = result.RawResponse
        };

        _app.Payments.Add(payment);
        _app.TransactionHistory.Add(new TransactionHistory
        {
            UserId = userId,
            OrderId = orden.OrderId,
            PaymentId = payment.PaymentId,
            Action = "CrearPagoPayPal",
            Details = $"Se creó la orden PayPal {result.OrderId} por ${orden.Total:N2}."
        });

        await _app.SaveChangesAsync();

        return Redirect(result.ApprovalUrl);
    }

    public async Task<IActionResult> PagoExitoso(string token)
    {
        if (string.IsNullOrWhiteSpace(token))
            return BadRequest("PayPal no devolvió token de orden.");

        var userId = _userManager.GetUserId(User);

        var payment = await _app.Payments
            .Include(p => p.Order)
            .ThenInclude(o => o.Details)
            .FirstOrDefaultAsync(p => p.Gateway == "PayPal" && p.ExternalTransactionId == token);

        if (payment == null)
            return NotFound();

        if (payment.Order.UserId != userId)
            return Forbid();

        if (payment.Status != "Aprobado")
        {
            var capture = await _payPalService.CaptureOrderAsync(token);

            payment.CaptureId = capture.CaptureId;
            payment.ResponseMessage = capture.RawResponse;

            if (capture.Status == "COMPLETED")
            {
                payment.Status = "Aprobado";
                payment.ConfirmationDate = DateTime.UtcNow;
                payment.Order.Status = "Aprobado";

                var detalle = payment.Order.Details.FirstOrDefault();
                var seat = detalle != null
                    ? await _app.Seats.FirstOrDefaultAsync(s => s.SeatId == detalle.SeatId)
                    : null;
                if (seat != null)
                    seat.IsOccupied = true;

                _app.TransactionHistory.Add(new TransactionHistory
                {
                    UserId = userId,
                    OrderId = payment.OrderId,
                    PaymentId = payment.PaymentId,
                    Action = "CapturarPayPal",
                    Details = $"Pago capturado por PayPal (capture {capture.CaptureId}) por ${payment.Amount:N2}."
                });
            }
            else if (capture.Status == "DECLINED" || capture.Status == "VOIDED")
            {
                payment.Status = "Rechazado";
                payment.Order.Status = "Rechazado";
            }
            else
            {
                payment.Status = "Fallido";
                payment.Order.Status = "Fallido";
            }

            await _app.SaveChangesAsync();
        }

        return RedirectToAction(nameof(Pago), new { id = payment.OrderId });
    }

    [HttpPost]
    public async Task<IActionResult> CreatePayPalButtonOrderJson(int orderId)
    {
        var userId = _userManager.GetUserId(User)!;

        var orden = await _app.Orders
            .Include(o => o.Details)
            .FirstOrDefaultAsync(o => o.OrderId == orderId && o.UserId == userId);

        if (orden == null)
        {
            return Json(new { success = false, message = "Orden no encontrada." });
        }

        if (orden.Total < 1.00m)
        {
            return Json(new { success = false, message = "El monto mínimo para pagar con PayPal Sandbox es de $1.00." });
        }

        string reference = $"Orden AirportApp #{orden.OrderId}";

        var result = await _payPalService.CreateOrderAsync(orden.Total, reference);

        var payment = new Payment
        {
            OrderId = orden.OrderId,
            UserId = userId,
            Gateway = "PayPal",
            ExternalTransactionId = result.OrderId,
            ApprovalUrl = result.ApprovalUrl,
            Amount = orden.Total,
            Currency = "USD",
            Status = "Pendiente",
            CreationDate = DateTime.UtcNow,
            ResponseMessage = result.RawResponse
        };

        _app.Payments.Add(payment);
        _app.TransactionHistory.Add(new TransactionHistory
        {
            UserId = userId,
            OrderId = orden.OrderId,
            PaymentId = payment.PaymentId,
            Action = "CrearPagoPayPal",
            Details = $"Se creó la orden PayPal {result.OrderId} por ${orden.Total:N2}."
        });

        await _app.SaveChangesAsync();

        return Json(new
        {
            success = true,
            paypalOrderId = result.OrderId,
            paymentTransactionId = payment.PaymentId
        });
    }

    [HttpPost]
    public async Task<IActionResult> CapturePayPalButtonOrderJson([FromBody] PayPalButtonCaptureRequest request)
    {
        var userId = _userManager.GetUserId(User)!;

        var payment = await _app.Payments
            .Include(p => p.Order)
            .ThenInclude(o => o.Details)
            .FirstOrDefaultAsync(p =>
                p.PaymentId == request.PaymentTransactionId &&
                p.ExternalTransactionId == request.PayPalOrderId);

        if (payment == null)
        {
            return Json(new { success = false, message = "Transacción no encontrada." });
        }

        if (payment.Status == "Aprobado")
        {
            return Json(new { success = true, redirectUrl = Url.Action(nameof(Pago), new { id = payment.OrderId }) });
        }

        var capture = await _payPalService.CaptureOrderAsync(request.PayPalOrderId);

        payment.CaptureId = capture.CaptureId;
        payment.ResponseMessage = capture.RawResponse;
        payment.ConfirmationDate = DateTime.UtcNow;

        if (capture.Status == "COMPLETED")
        {
            payment.Status = "Aprobado";
            payment.Order.Status = "Aprobado";

            var detalle = payment.Order.Details.FirstOrDefault();
            var seat = detalle != null
                ? await _app.Seats.FirstOrDefaultAsync(s => s.SeatId == detalle.SeatId)
                : null;
            if (seat != null)
                seat.IsOccupied = true;

            _app.TransactionHistory.Add(new TransactionHistory
            {
                UserId = userId,
                OrderId = payment.OrderId,
                PaymentId = payment.PaymentId,
                Action = "CapturarPayPal",
                Details = $"Pago capturado por PayPal (capture {capture.CaptureId}) por ${payment.Amount:N2}."
            });
        }
        else if (capture.Status == "DECLINED" || capture.Status == "VOIDED")
        {
            payment.Status = "Rechazado";
            payment.Order.Status = "Rechazado";
        }
        else
        {
            payment.Status = "Fallido";
            payment.Order.Status = "Fallido";
        }

        await _app.SaveChangesAsync();

        return Json(new
        {
            success = true,
            redirectUrl = Url.Action(nameof(Pago), new { id = payment.OrderId })
        });
    }

    public class PayPalButtonCaptureRequest
    {
        public string PayPalOrderId { get; set; } = string.Empty;
        public int PaymentTransactionId { get; set; }
    }

    public async Task<IActionResult> PagoCancelado(string token)
    {
        if (!string.IsNullOrWhiteSpace(token))
        {
            var payment = await _app.Payments
                .Include(p => p.Order)
                .FirstOrDefaultAsync(p => p.Gateway == "PayPal" && p.ExternalTransactionId == token);

            if (payment != null && payment.Status == "Pendiente")
            {
                payment.Status = "Cancelado";
                payment.Order.Status = "Cancelado";
                await _app.SaveChangesAsync();
            }
        }

        TempData["Error"] = "El pago con PayPal fue cancelado.";
        return RedirectToAction(nameof(Historial));
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> CrearPagoPayPhone(int orderId)
    {
        var userId = _userManager.GetUserId(User)!;

        var orden = await _app.Orders
            .Include(o => o.Details)
            .FirstOrDefaultAsync(o => o.OrderId == orderId && o.UserId == userId);

        if (orden == null)
            return NotFound();

        string clientTransactionId = DateTime.Now.ToString("yyMMddHHmmssfff")[..15];
        string reference = $"Orden AirportApp #{orden.OrderId}";

        string link = await _payPhoneService.CreatePaymentLinkAsync(
            orden.Total,
            clientTransactionId,
            reference);

        var payment = new Payment
        {
            OrderId = orden.OrderId,
            UserId = userId,
            Gateway = "PayPhone",
            ExternalTransactionId = clientTransactionId,
            ApprovalUrl = link,
            Amount = orden.Total,
            Currency = "USD",
            Status = "Pendiente",
            CreationDate = DateTime.UtcNow,
            ResponseMessage = "Link de pago generado."
        };

        _app.Payments.Add(payment);
        _app.TransactionHistory.Add(new TransactionHistory
        {
            UserId = userId,
            OrderId = orden.OrderId,
            PaymentId = payment.PaymentId,
            Action = "CrearLinkPayPhone",
            Details = $"Se generó link de pago PayPhone ({clientTransactionId}) por ${orden.Total:N2}."
        });

        await _app.SaveChangesAsync();

        return RedirectToAction(nameof(Pago), new { id = orden.OrderId });
    }

    public async Task<IActionResult> Historial()
    {
        var userId = _userManager.GetUserId(User);

        var ordenes = await _app.Orders
            .AsNoTracking()
            .Include(o => o.Details)
            .Where(o => o.UserId == userId)
            .OrderByDescending(o => o.CreatedAt)
            .Take(20)
            .ToListAsync();

        return View(ordenes);
    }
}

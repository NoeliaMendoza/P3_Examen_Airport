using Microsoft.EntityFrameworkCore;
using AirportApp.Data;
using AirportApp.Models;
using AirportApp.Models.Commerce;

namespace AirportApp.Services;

public class SeatService
{
    private readonly AirportContext _airport;
    private readonly ApplicationDbContext _app;

    public SeatService(AirportContext airport, ApplicationDbContext app)
    {
        _airport = airport;
        _app = app;
    }

    public async Task<Flight?> GetFlightAsync(int flightId)
    {
        return await _airport.Flights
            .AsNoTracking()
            .Include(f => f.Airplane)
            .Include(f => f.Airplane!.Type)
            .Include(f => f.FromNavigation)
            .Include(f => f.ToNavigation)
            .FirstOrDefaultAsync(f => f.FlightId == flightId);
    }

    public async Task<List<Seat>> GetSeatsAsync(int flightId)
    {
        var seats = await _app.Seats
            .AsNoTracking()
            .Where(s => s.FlightId == flightId)
            .OrderBy(s => s.SeatNo)
            .ToListAsync();

        if (seats.Count > 0)
            return seats;

        var flight = await GetFlightAsync(flightId);
        if (flight?.Airplane == null)
            return new List<Seat>();

        seats = GenerateSeats(flight);
        _app.Seats.AddRange(seats);
        await _app.SaveChangesAsync();
        return seats;
    }

    private static List<Seat> GenerateSeats(Flight flight)
    {
        var seats = new List<Seat>();
        int capacity = flight.Airplane!.Capacity;
        int rows = (int)Math.Ceiling(capacity / 6.0);

        for (int row = 1; row <= rows; row++)
        {
            string seatClass = row <= 2 ? "First" : row <= 5 ? "Business" : "Economy";
            decimal price = seatClass == "First" ? 850.00m : seatClass == "Business" ? 450.00m : 180.00m;

            for (int col = 1; col <= 6; col++)
            {
                int n = (row - 1) * 6 + col;
                if (n > capacity)
                    break;

                seats.Add(new Seat
                {
                    FlightId = flight.FlightId,
                    SeatNo = $"{row}{(char)(64 + col)}",
                    SeatClass = seatClass,
                    IsOccupied = false,
                    Price = price,
                });
            }
        }

        return seats;
    }

    public async Task<Seat?> FindSeatAsync(int flightId, string seatNo)
    {
        return await _app.Seats
            .AsNoTracking()
            .FirstOrDefaultAsync(s => s.FlightId == flightId && s.SeatNo == seatNo);
    }

    public async Task<bool> SeatIsTakenAsync(int flightId, int seatId, string? exceptUserId = null)
    {
        var seat = await _app.Seats.FirstOrDefaultAsync(s => s.SeatId == seatId && s.FlightId == flightId);
        if (seat == null)
            return true;

        if (seat.IsOccupied)
            return true;

        return await _app.SeatReservations.AnyAsync(r =>
            r.SeatId == seatId && !r.IsConfirmed && r.ExpiresAt > DateTime.UtcNow
            && r.UserId != exceptUserId);
    }
}

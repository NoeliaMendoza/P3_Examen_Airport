using System;
using System.Collections.Generic;

namespace AirportApp.Models;

/// <summary>
/// Flughafen DB by Stefan Pröll, Eva Zangerle, Wolfgang Gassler is licensed under CC BY 4.0. To view a copy of this license, visit https://creativecommons.org/licenses/by/4.0
/// </summary>
public partial class Booking
{
    public int BookingId { get; set; }

    public int FlightId { get; set; }

    public string? Seat { get; set; }

    public int PassengerId { get; set; }

    public decimal Price { get; set; }

    public virtual Flight Flight { get; set; } = null!;

    public virtual Passenger Passenger { get; set; } = null!;
}

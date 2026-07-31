using System;
using System.Collections.Generic;

namespace AirportApp.Models;

/// <summary>
/// Flughafen DB by Stefan Pröll, Eva Zangerle, Wolfgang Gassler is licensed under CC BY 4.0. To view a copy of this license, visit https://creativecommons.org/licenses/by/4.0
/// </summary>
public partial class Flight
{
    public int FlightId { get; set; }

    public string Flightno { get; set; } = null!;

    public int From { get; set; }

    public int To { get; set; }

    public DateTime Departure { get; set; }

    public DateTime Arrival { get; set; }

    public int AirlineId { get; set; }

    public int AirplaneId { get; set; }

    public virtual Airline Airline { get; set; } = null!;

    public virtual Airplane Airplane { get; set; } = null!;

    public virtual ICollection<Booking> Bookings { get; set; } = new List<Booking>();

    public virtual ICollection<FlightLog> FlightLogs { get; set; } = new List<FlightLog>();

    public virtual Flightschedule FlightnoNavigation { get; set; } = null!;

    public virtual Airport FromNavigation { get; set; } = null!;

    public virtual Airport ToNavigation { get; set; } = null!;
}

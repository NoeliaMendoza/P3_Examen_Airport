using System;
using System.Collections.Generic;

namespace AirportApp.Models;

/// <summary>
/// Flughafen DB by Stefan Pröll, Eva Zangerle, Wolfgang Gassler is licensed under CC BY 4.0. To view a copy of this license, visit https://creativecommons.org/licenses/by/4.0
/// </summary>
public partial class Airline
{
    public int AirlineId { get; set; }

    public string Iata { get; set; } = null!;

    public string? Airlinename { get; set; }

    public int BaseAirport { get; set; }

    public virtual ICollection<Flight> Flights { get; set; } = new List<Flight>();

    public virtual ICollection<Flightschedule> Flightschedules { get; set; } = new List<Flightschedule>();
}

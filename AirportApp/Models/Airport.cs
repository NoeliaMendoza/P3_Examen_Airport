using System;
using System.Collections.Generic;

namespace AirportApp.Models;

/// <summary>
/// Flughafen DB by Stefan Pröll, Eva Zangerle, Wolfgang Gassler is licensed under CC BY 4.0. To view a copy of this license, visit https://creativecommons.org/licenses/by/4.0
/// </summary>
public partial class Airport
{
    public int AirportId { get; set; }

    public string? Iata { get; set; }

    public string Icao { get; set; } = null!;

    public string Name { get; set; } = null!;

    public virtual AirportGeo? AirportGeo { get; set; }

    public virtual AirportReachable? AirportReachable { get; set; }

    public virtual ICollection<Flight> FlightFromNavigations { get; set; } = new List<Flight>();

    public virtual ICollection<Flight> FlightToNavigations { get; set; } = new List<Flight>();

    public virtual ICollection<Flightschedule> FlightscheduleFromNavigations { get; set; } = new List<Flightschedule>();

    public virtual ICollection<Flightschedule> FlightscheduleToNavigations { get; set; } = new List<Flightschedule>();
}

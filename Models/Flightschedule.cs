using System;
using System.Collections.Generic;

namespace AirportApp.Models;

/// <summary>
/// Flughafen DB by Stefan Pröll, Eva Zangerle, Wolfgang Gassler is licensed under CC BY 4.0. To view a copy of this license, visit https://creativecommons.org/licenses/by/4.0
/// </summary>
public partial class Flightschedule
{
    public string Flightno { get; set; } = null!;

    public int From { get; set; }

    public int To { get; set; }

    public TimeOnly Departure { get; set; }

    public TimeOnly Arrival { get; set; }

    public int AirlineId { get; set; }

    public bool? Monday { get; set; }

    public bool? Tuesday { get; set; }

    public bool? Wednesday { get; set; }

    public bool? Thursday { get; set; }

    public bool? Friday { get; set; }

    public bool? Saturday { get; set; }

    public bool? Sunday { get; set; }

    public virtual Airline Airline { get; set; } = null!;

    public virtual ICollection<Flight> Flights { get; set; } = new List<Flight>();

    public virtual Airport FromNavigation { get; set; } = null!;

    public virtual Airport ToNavigation { get; set; } = null!;
}

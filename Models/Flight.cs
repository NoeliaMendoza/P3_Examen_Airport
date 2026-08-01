using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace AirportApp.Models;

/// <summary>
/// Flughafen DB by Stefan Pröll, Eva Zangerle, Wolfgang Gassler is licensed under CC BY 4.0. To view a copy of this license, visit https://creativecommons.org/licenses/by/4.0
/// </summary>
public partial class Flight
{
    public int FlightId { get; set; }

    [Required(ErrorMessage = "El campo {0} es obligatorio.")]
    [Display(Name = "N.º de vuelo")]
    public string Flightno { get; set; } = null!;

    [Range(1, int.MaxValue, ErrorMessage = "El campo {0} es obligatorio.")]
    [Display(Name = "Origen")]
    public int From { get; set; }

    [Range(1, int.MaxValue, ErrorMessage = "El campo {0} es obligatorio.")]
    [Display(Name = "Destino")]
    public int To { get; set; }

    [Required(ErrorMessage = "El campo {0} es obligatorio.")]
    [Display(Name = "Salida")]
    public DateTime Departure { get; set; }

    [Required(ErrorMessage = "El campo {0} es obligatorio.")]
    [Display(Name = "Llegada")]
    public DateTime Arrival { get; set; }

    [Range(1, int.MaxValue, ErrorMessage = "Seleccione la aerolínea.")]
    [Display(Name = "Aerolínea")]
    public int AirlineId { get; set; }

    [Range(1, int.MaxValue, ErrorMessage = "Seleccione la aeronave.")]
    [Display(Name = "Aeronave")]
    public int AirplaneId { get; set; }

    public virtual Airline Airline { get; set; } = null!;

    public virtual Airplane Airplane { get; set; } = null!;

    public virtual ICollection<Booking> Bookings { get; set; } = new List<Booking>();

    public virtual ICollection<FlightLog> FlightLogs { get; set; } = new List<FlightLog>();

    public virtual Flightschedule FlightnoNavigation { get; set; } = null!;

    public virtual Airport FromNavigation { get; set; } = null!;

    public virtual Airport ToNavigation { get; set; } = null!;
}

using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace AirportApp.Models;

/// <summary>
/// Flughafen DB by Stefan Pröll, Eva Zangerle, Wolfgang Gassler is licensed under CC BY 4.0. To view a copy of this license, visit https://creativecommons.org/licenses/by/4.0
/// </summary>
public partial class Airline
{
    public int AirlineId { get; set; }

    [Display(Name = "Código IATA")]
    [StringLength(2, ErrorMessage = "El campo {0} debe tener exactamente {1} caracteres.")]
    public string Iata { get; set; } = null!;

    [Required(ErrorMessage = "El campo {0} es obligatorio.")]
    [StringLength(30, ErrorMessage = "El campo {0} no puede superar los {1} caracteres.")]
    [Display(Name = "Nombre de la aerolínea")]
    public string? Airlinename { get; set; }

    [Display(Name = "Aeropuerto base")]
    public int BaseAirport { get; set; }

    public virtual ICollection<Flight> Flights { get; set; } = new List<Flight>();

    public virtual ICollection<Flightschedule> Flightschedules { get; set; } = new List<Flightschedule>();
}

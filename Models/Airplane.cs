using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace AirportApp.Models;

/// <summary>
/// Flughafen DB by Stefan Pröll, Eva Zangerle, Wolfgang Gassler is licensed under CC BY 4.0. To view a copy of this license, visit https://creativecommons.org/licenses/by/4.0
/// </summary>
public partial class Airplane
{
    public int AirplaneId { get; set; }

    [Range(1, int.MaxValue, ErrorMessage = "El campo {0} es obligatorio.")]
    [Display(Name = "Capacidad")]
    public int Capacity { get; set; }

    [Range(1, int.MaxValue, ErrorMessage = "Seleccione el tipo de aeronave.")]
    [Display(Name = "Tipo de aeronave")]
    public int TypeId { get; set; }

    [Range(1, int.MaxValue, ErrorMessage = "Seleccione la aerolínea.")]
    [Display(Name = "Aerolínea")]
    public int AirlineId { get; set; }

    public virtual ICollection<Flight> Flights { get; set; } = new List<Flight>();

    public virtual AirplaneType Type { get; set; } = null!;
}

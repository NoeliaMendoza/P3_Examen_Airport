using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace AirportApp.Models;

/// <summary>
/// Flughafen DB by Stefan Pröll, Eva Zangerle, Wolfgang Gassler is licensed under CC BY 4.0. To view a copy of this license, visit https://creativecommons.org/licenses/by/4.0
/// </summary>
public partial class Passenger
{
    public int PassengerId { get; set; }

    [Required(ErrorMessage = "El campo {0} es obligatorio.")]
    [Display(Name = "N.º de pasaporte")]
    public string Passportno { get; set; } = null!;

    [Required(ErrorMessage = "El campo {0} es obligatorio.")]
    [Display(Name = "Nombre")]
    public string Firstname { get; set; } = null!;

    [Required(ErrorMessage = "El campo {0} es obligatorio.")]
    [Display(Name = "Apellido")]
    public string Lastname { get; set; } = null!;

    public virtual ICollection<Booking> Bookings { get; set; } = new List<Booking>();

    public virtual Passengerdetail? Passengerdetail { get; set; }
}

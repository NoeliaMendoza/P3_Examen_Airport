using System;
using System.Collections.Generic;

namespace AirportApp.Models;

/// <summary>
/// Flughafen DB by Stefan Pröll, Eva Zangerle, Wolfgang Gassler is licensed under CC BY 4.0. To view a copy of this license, visit https://creativecommons.org/licenses/by/4.0
/// </summary>
public partial class AirplaneType
{
    public int TypeId { get; set; }

    public string? Identifier { get; set; }

    public string? Description { get; set; }

    public virtual ICollection<Airplane> Airplanes { get; set; } = new List<Airplane>();
}

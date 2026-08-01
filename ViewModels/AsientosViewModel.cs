using AirportApp.Models;
using AirportApp.Models.Commerce;

namespace AirportApp.ViewModels;

public class AsientosViewModel
{
    public Flight? Vuelo { get; set; }
    public List<Seat> Asientos { get; set; } = new();
    public string? Clase { get; set; }
    public decimal PrecioPromedio { get; set; }
    public int Disponibles { get; set; }
    public int Ocupados { get; set; }

    public IEnumerable<Seat> AsientosFiltrados =>
        string.IsNullOrEmpty(Clase) ? Asientos : Asientos.Where(s => s.SeatClass == Clase);
}

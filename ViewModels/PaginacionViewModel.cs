namespace AirportApp.ViewModels;

public class PaginacionViewModel
{
    public int PaginaActual { get; set; } = 1;
    public int TotalPaginas { get; set; }
    public int TotalRegistros { get; set; }
    public int TamanoPagina { get; set; } = 20;
    public string? Buscar { get; set; }
    public string? Filtro1 { get; set; }
    public string? Filtro2 { get; set; }
    public string? Orden { get; set; }
    public bool TienePaginaAnterior => PaginaActual > 1;
    public bool TienePaginaSiguiente => PaginaActual < TotalPaginas;
}

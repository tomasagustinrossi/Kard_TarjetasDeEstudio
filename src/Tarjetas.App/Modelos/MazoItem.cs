using CommunityToolkit.Mvvm.ComponentModel;
using Tarjetas.Aplicacion;

namespace Tarjetas.App.Modelos;

/// <summary>Cómo se ve un mazo en la pantalla principal.</summary>
public partial class MazoItem : ObservableObject
{
    private MazoItem() { }

    public Guid Id { get; private init; }
    public string Nombre { get; private init; } = "";
    public string Resumen { get; private init; } = "";
    public string? Pendientes { get; private init; }
    public int Total { get; private init; }
    public Color Fondo { get; private init; } = Colors.Transparent;
    public Brush Borde { get; private init; } = Brush.Transparent;
    public Color FondoEtiqueta { get; private init; } = Colors.Transparent;
    public bool TienePendientes => Pendientes is not null;

    /// <summary>Mientras es verdadero, el cuadro tiembla (lo hace <c>TemblorBehavior</c>).</summary>
    [ObservableProperty] private bool enEdicion;

    public static MazoItem De(MazoResumen m, bool enEdicion) => new()
    {
        Id = m.Id,
        Nombre = m.Nombre,
        Total = m.Total,
        Resumen = m.Total == 1 ? "1 tarjeta" : $"{m.Total} tarjetas",
        Pendientes = m.Pendientes > 0 ? $"{m.Pendientes} para hoy" : null,
        Fondo = PaletaKeep.Fondo(m.Color),
        Borde = PaletaKeep.Pincel(PaletaKeep.Borde(m.Color)),
        FondoEtiqueta = PaletaKeep.FondoEtiqueta,
        EnEdicion = enEdicion
    };
}

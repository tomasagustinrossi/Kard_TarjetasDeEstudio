using Tarjetas.Aplicacion;

namespace Tarjetas.App.Modelos;

/// <summary>Cómo se ve un mazo en la pantalla principal.</summary>
public sealed record MazoItem(
    Guid Id, string Nombre, string Resumen, string? Pendientes, Color Fondo, Brush Borde, Color FondoEtiqueta)
{
    public bool TienePendientes => Pendientes is not null;

    public static MazoItem De(MazoResumen m) => new(
        m.Id,
        m.Nombre,
        m.Total == 1 ? "1 tarjeta" : $"{m.Total} tarjetas",
        m.Pendientes > 0 ? $"{m.Pendientes} para hoy" : null,
        PaletaKeep.Fondo(m.Color),
        PaletaKeep.Pincel(PaletaKeep.Borde(m.Color)),
        PaletaKeep.FondoEtiqueta);
}

using Tarjetas.Aplicacion;
using Tarjetas.Dominio;

namespace Tarjetas.App.Modelos;

/// <summary>Cómo se ve una tarjeta como "cuadrado" dentro de un mazo: pregunta, respuesta u opciones y estado.</summary>
public sealed record TarjetaItem(
    Guid Id, string Pregunta, string Detalle, string? Etiqueta, Color Fondo, Brush Borde, Color FondoEtiqueta)
{
    public bool TieneEtiqueta => Etiqueta is not null;

    public static TarjetaItem De(TarjetaVista v, ColorMazo color) => new(
        v.Id,
        v.Pregunta,
        Detallar(v),
        EtiquetaDe(v),
        PaletaKeep.Fondo(color),
        PaletaKeep.Pincel(PaletaKeep.Borde(color)),
        PaletaKeep.FondoEtiqueta);

    private static string Detallar(TarjetaVista v) => v.Tipo switch
    {
        TipoRespuesta.Escrita => v.Respuesta,
        TipoRespuesta.OpcionUnica => string.Join('\n', v.Opciones.Select(o => $"{(o.EsCorrecta ? "●" : "○")} {o.Texto}")),
        _ => string.Join('\n', v.Opciones.Select(o => $"{(o.EsCorrecta ? "☑" : "☐")} {o.Texto}"))
    };

    private static string? EtiquetaDe(TarjetaVista v) => v switch
    {
        { DeudaDias: int dias } => $"Debes -{dias} d",
        { EsNueva: true } => "Nueva",
        { DebeRepasarse: true } => "Para hoy",
        _ => null
    };
}

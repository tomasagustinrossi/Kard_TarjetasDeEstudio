using Tarjetas.Dominio;

namespace Tarjetas.Aplicacion;

/// <summary>Lo que la interfaz necesita saber de una tarjeta, ya calculado para "hoy".</summary>
public sealed record TarjetaVista(
    Guid Id,
    TipoRespuesta Tipo,
    string Pregunta,
    string Respuesta,
    IReadOnlyList<Opcion> Opciones,
    bool EsNueva,
    bool DebeRepasarse,
    int? DeudaDias,
    DateOnly ProximoRepaso)
{
    internal static TarjetaVista De(Tarjeta t, DateOnly hoy) => new(
        t.Id, t.Tipo, t.Pregunta, t.Respuesta, t.Opciones,
        t.EsNueva, t.DebeRepasarse(hoy), t.DeudaVisible(hoy), t.ProximoRepaso(hoy));
}

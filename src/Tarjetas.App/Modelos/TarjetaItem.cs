using CommunityToolkit.Mvvm.ComponentModel;
using Tarjetas.Aplicacion;
using Tarjetas.Dominio;

namespace Tarjetas.App.Modelos;

/// <summary>
/// Un "cuadrado" de tarjeta dentro de un mazo. La respuesta aparece pixelada (▒▒▒) hasta que se toca la tarjeta.
/// </summary>
public partial class TarjetaItem : ObservableObject
{
    private string detalleOculto = "";
    private string detalleVisible = "";

    private TarjetaItem() { }

    public Guid Id { get; private init; }
    public string Pregunta { get; private init; } = "";
    public string? Etiqueta { get; private init; }
    public Color Fondo { get; private init; } = Colors.Transparent;
    public Brush Borde { get; private init; } = Brush.Transparent;
    public Color FondoEtiqueta { get; private init; } = Colors.Transparent;
    public bool TieneEtiqueta => Etiqueta is not null;

    public string Detalle => Revelada ? detalleVisible : detalleOculto;

    [ObservableProperty]
    [NotifyPropertyChangedFor(nameof(Detalle))]
    private bool revelada;

    [ObservableProperty] private bool enEdicion;

    public static TarjetaItem De(TarjetaVista v, ColorMazo color, bool enEdicion) => new()
    {
        Id = v.Id,
        Pregunta = v.Pregunta,
        Etiqueta = EtiquetaDe(v),
        Fondo = PaletaKeep.Fondo(color),
        Borde = PaletaKeep.Pincel(PaletaKeep.Borde(color)),
        FondoEtiqueta = PaletaKeep.FondoEtiqueta,
        detalleOculto = Ocultar(v),
        detalleVisible = Mostrar(v),
        EnEdicion = enEdicion
    };

    private static string Ocultar(TarjetaVista v) => v.Tipo switch
    {
        TipoRespuesta.Escrita => Pixelar(v.Respuesta),
        TipoRespuesta.OpcionUnica => string.Join('\n', v.Opciones.Select(o => $"○ {o.Texto}")),
        _ => string.Join('\n', v.Opciones.Select(o => $"☐ {o.Texto}"))
    };

    private static string Mostrar(TarjetaVista v) => v.Tipo switch
    {
        TipoRespuesta.Escrita => v.Respuesta,
        TipoRespuesta.OpcionUnica => string.Join('\n', v.Opciones.Select(o => $"{(o.EsCorrecta ? "●" : "○")} {o.Texto}")),
        _ => string.Join('\n', v.Opciones.Select(o => $"{(o.EsCorrecta ? "☑" : "☐")} {o.Texto}"))
    };

    /// <summary>Conserva la forma de las palabras pero no deja leer nada.</summary>
    private static string Pixelar(string texto)
    {
        var palabras = texto.Split((char[]?)null, StringSplitOptions.RemoveEmptyEntries)
            .Select(p => new string('▒', Math.Min(p.Length, 10)));
        var pixelado = string.Join(' ', palabras);
        return pixelado.Length > 90 ? pixelado[..90] + "…" : pixelado;
    }

    private static string? EtiquetaDe(TarjetaVista v) => v switch
    {
        { DeudaDias: int dias } => $"Debes -{dias} d",
        { EsNueva: true } => "Nueva",
        { DebeRepasarse: true } => "Para hoy",
        _ => null
    };
}

using Tarjetas.Dominio;

namespace Tarjetas.Aplicacion;

public sealed record OpcionDatos(string Texto, bool EsCorrecta);

/// <summary>
/// Datos para crear/editar una tarjeta. Una tarjeta simple "pregunta/respuesta" (como las de Gemini)
/// se crea con <see cref="Escrita"/>; los tipos con opciones son una extensión que no la afecta.
/// </summary>
public sealed record TarjetaDatos(
    TipoRespuesta Tipo,
    string Pregunta,
    string Respuesta,
    IReadOnlyList<OpcionDatos>? Opciones = null)
{
    public static TarjetaDatos Escrita(string pregunta, string respuesta) =>
        new(TipoRespuesta.Escrita, pregunta, respuesta);
}

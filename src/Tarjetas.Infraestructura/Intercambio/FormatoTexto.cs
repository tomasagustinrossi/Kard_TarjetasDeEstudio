using Tarjetas.Aplicacion;
using Tarjetas.Dominio;

namespace Tarjetas.Infraestructura.Intercambio;

/// <summary>
/// Texto libre como el que devuelve Gemini (tablas, "Pregunta/Respuesta", "Q/A", opciones A) B) C)…).
/// Solo lee; es el formato de último recurso, por eso se registra al final.
/// </summary>
public sealed class FormatoTexto : IFormatoDeIntercambio
{
    public string Id => "texto";
    public string NombreVisible => "Texto (Gemini, Markdown)";
    public string Extension => ".txt";
    public bool PuedeEscribir => false;

    public bool PuedeLeer(string nombreArchivo, string contenido) => true;

    public MazoDeIntercambio Leer(string contenido, string nombreSugerido) =>
        new(nombreSugerido, ColorMazo.Predeterminado, ParserDeTexto.Parsear(contenido));

    public string Escribir(MazoDeIntercambio mazo) => throw new NotSupportedException();
}

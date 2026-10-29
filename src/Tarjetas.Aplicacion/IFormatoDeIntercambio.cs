using Tarjetas.Dominio;

namespace Tarjetas.Aplicacion;

/// <summary>Un mazo "en tránsito": lo que se importa o exporta, sin identidades ni progreso de repaso.</summary>
public sealed record MazoDeIntercambio(string Nombre, ColorMazo Color, IReadOnlyList<TarjetaDatos> Tarjetas);

public sealed record Previsualizacion(MazoDeIntercambio Mazo, int Descartadas);

public sealed record ArchivoExportado(string NombreArchivo, string Contenido);

public sealed class ExcepcionDeImportacion(string mensaje) : Exception(mensaje);

/// <summary>
/// Puerto para leer/escribir mazos en un formato de archivo. La infraestructura aporta una implementación
/// por formato (nativo, CSV, texto de Gemini…) sin que el dominio sepa nada de ellos.
/// </summary>
public interface IFormatoDeIntercambio
{
    string Id { get; }
    string NombreVisible { get; }
    string Extension { get; }
    bool PuedeEscribir { get; }

    bool PuedeLeer(string nombreArchivo, string contenido);
    MazoDeIntercambio Leer(string contenido, string nombreSugerido);
    string Escribir(MazoDeIntercambio mazo);
}

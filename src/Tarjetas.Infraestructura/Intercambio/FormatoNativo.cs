using System.Text.Encodings.Web;
using System.Text.Json;
using System.Text.Json.Serialization;
using Tarjetas.Aplicacion;
using Tarjetas.Dominio;

namespace Tarjetas.Infraestructura.Intercambio;

/// <summary>
/// Formato propio de la app para compartir mazos: JSON legible, con tipos, opciones y justificaciones.
/// (No incluye el progreso de repaso: quien importa empieza de cero.)
/// </summary>
public sealed class FormatoNativo : IFormatoDeIntercambio
{
    private static readonly JsonSerializerOptions Opciones = new()
    {
        PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
        WriteIndented = true,
        DefaultIgnoreCondition = JsonIgnoreCondition.WhenWritingNull,
        Encoder = JavaScriptEncoder.UnsafeRelaxedJsonEscaping
    };

    public string Id => "nativo";
    public string NombreVisible => "Archivo Tarjetas (.tarjetas.json)";
    public string Extension => ".tarjetas.json";
    public bool PuedeEscribir => true;

    public bool PuedeLeer(string nombreArchivo, string contenido)
    {
        if (!contenido.TrimStart().StartsWith('{')) return false;
        try
        {
            using var documento = JsonDocument.Parse(contenido);
            return documento.RootElement.ValueKind == JsonValueKind.Object
                   && documento.RootElement.TryGetProperty("tarjetas", out _);
        }
        catch (JsonException)
        {
            return false;
        }
    }

    public MazoDeIntercambio Leer(string contenido, string nombreSugerido)
    {
        ArchivoMazo? archivo;
        try
        {
            archivo = JsonSerializer.Deserialize<ArchivoMazo>(contenido, Opciones);
        }
        catch (JsonException)
        {
            throw new ExcepcionDeImportacion("El archivo no tiene el formato de Tarjetas.");
        }
        if (archivo is null) throw new ExcepcionDeImportacion("El archivo está vacío.");

        var color = Enum.TryParse<ColorMazo>(archivo.Color, ignoreCase: true, out var c) ? c : ColorMazo.Predeterminado;
        var tarjetas = (archivo.Tarjetas ?? []).Select(ADatos).ToList();
        return new MazoDeIntercambio(string.IsNullOrWhiteSpace(archivo.Nombre) ? nombreSugerido : archivo.Nombre, color, tarjetas);
    }

    public string Escribir(MazoDeIntercambio mazo)
    {
        var archivo = new ArchivoMazo
        {
            Nombre = mazo.Nombre,
            Color = mazo.Color.ToString(),
            Tarjetas = mazo.Tarjetas.Select(t => new ArchivoTarjeta
            {
                Tipo = TipoComoTexto(t.Tipo),
                Pregunta = t.Pregunta,
                Respuesta = string.IsNullOrEmpty(t.Respuesta) ? null : t.Respuesta,
                Opciones = t.Opciones is { Count: > 0 }
                    ? t.Opciones.Select(o => new ArchivoOpcion
                    {
                        Texto = o.Texto,
                        Correcta = o.EsCorrecta,
                        Justificacion = string.IsNullOrEmpty(o.Justificacion) ? null : o.Justificacion
                    }).ToList()
                    : null
            }).ToList()
        };
        return JsonSerializer.Serialize(archivo, Opciones);
    }

    private static TarjetaDatos ADatos(ArchivoTarjeta t)
    {
        var tipo = (t.Tipo ?? "escrita").ToLowerInvariant() switch
        {
            "unica" or "única" => TipoRespuesta.OpcionUnica,
            "multiple" or "múltiple" => TipoRespuesta.OpcionMultiple,
            _ => TipoRespuesta.Escrita
        };
        var opciones = tipo == TipoRespuesta.Escrita
            ? null
            : (t.Opciones ?? []).Select(o => new OpcionDatos(o.Texto ?? "", o.Correcta, o.Justificacion ?? "")).ToList();
        return new TarjetaDatos(tipo, t.Pregunta ?? "", t.Respuesta ?? "", opciones);
    }

    private static string TipoComoTexto(TipoRespuesta tipo) => tipo switch
    {
        TipoRespuesta.OpcionUnica => "unica",
        TipoRespuesta.OpcionMultiple => "multiple",
        _ => "escrita"
    };

    private sealed class ArchivoMazo
    {
        public string Formato { get; set; } = "tarjetas";
        public int Version { get; set; } = 1;
        public string? Nombre { get; set; }
        public string? Color { get; set; }
        public List<ArchivoTarjeta>? Tarjetas { get; set; }
    }

    private sealed class ArchivoTarjeta
    {
        public string? Tipo { get; set; }
        public string? Pregunta { get; set; }
        public string? Respuesta { get; set; }
        public List<ArchivoOpcion>? Opciones { get; set; }
    }

    private sealed class ArchivoOpcion
    {
        public string? Texto { get; set; }
        public bool Correcta { get; set; }
        public string? Justificacion { get; set; }
    }
}

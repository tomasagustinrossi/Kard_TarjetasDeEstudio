using System.Text.Json.Serialization;
using Tarjetas.Dominio;
using Tarjetas.Dominio.Repasos;

namespace Tarjetas.Infraestructura;

/// <summary>
/// Formato en disco, pensado para pesar lo mínimo: nombres de campo de una letra, sin espacios,
/// y los valores por defecto (nivel 0, tipo escrita, sin opciones…) directamente no se escriben.
/// </summary>
internal sealed record MazoRegistro(
    [property: JsonPropertyName("i")] Guid Id,
    [property: JsonPropertyName("n")] string? Nombre,
    [property: JsonPropertyName("c")] byte Color,
    [property: JsonPropertyName("f")] long Creado,
    [property: JsonPropertyName("t")] TarjetaRegistro[]? Tarjetas)
{
    public static MazoRegistro De(Mazo m) => new(
        m.Id, m.Nombre, (byte)m.Color, new DateTimeOffset(DateTime.SpecifyKind(m.CreadoUtc, DateTimeKind.Utc)).ToUnixTimeSeconds(),
        m.Tarjetas.Select(TarjetaRegistro.De).ToArray());

    public Mazo ADominio() => Mazo.Rehidratar(
        Id, Nombre ?? "", (ColorMazo)Color, DateTimeOffset.FromUnixTimeSeconds(Creado).UtcDateTime,
        (Tarjetas ?? []).Select(t => t.ADominio()));
}

internal sealed record TarjetaRegistro(
    [property: JsonPropertyName("i")] Guid Id,
    [property: JsonPropertyName("p")] string? Pregunta,
    [property: JsonPropertyName("r")] string? Respuesta,
    [property: JsonPropertyName("k")] byte Tipo,
    [property: JsonPropertyName("o")] string[]? Opciones,
    [property: JsonPropertyName("a")] int[]? Correctas,
    [property: JsonPropertyName("j")] string[]? Justificaciones,
    [property: JsonPropertyName("f")] long Creada,
    [property: JsonPropertyName("v")] int Nivel,
    [property: JsonPropertyName("u")] int? UltimoRepaso)
{
    public static TarjetaRegistro De(Tarjeta t)
    {
        var hayOpciones = t.Opciones.Count > 0;
        return new TarjetaRegistro(
            t.Id,
            t.Pregunta,
            t.Respuesta.Length == 0 ? null : t.Respuesta,
            (byte)t.Tipo,
            hayOpciones ? t.Opciones.Select(o => o.Texto).ToArray() : null,
            hayOpciones ? t.Opciones.Select((o, i) => (o, i)).Where(x => x.o.EsCorrecta).Select(x => x.i).ToArray() : null,
            hayOpciones && t.Opciones.Any(o => o.Justificacion.Length > 0) ? t.Opciones.Select(o => o.Justificacion).ToArray() : null,
            new DateTimeOffset(DateTime.SpecifyKind(t.CreadaUtc, DateTimeKind.Utc)).ToUnixTimeSeconds(),
            t.Repaso.Nivel,
            t.Repaso.UltimoRepaso?.DayNumber);
    }

    public Tarjeta ADominio()
    {
        var opciones = (Opciones ?? [])
            .Select((texto, i) => new Opcion(texto, Correctas?.Contains(i) == true, Justificaciones?.ElementAtOrDefault(i) ?? ""))
            .ToList();
        return Tarjeta.Rehidratar(
            Id, (TipoRespuesta)Tipo, Pregunta ?? "", Respuesta ?? "", opciones,
            DateTimeOffset.FromUnixTimeSeconds(Creada).UtcDateTime,
            new ProgramaDeRepaso(Nivel, UltimoRepaso is { } d ? DateOnly.FromDayNumber(d) : null));
    }
}

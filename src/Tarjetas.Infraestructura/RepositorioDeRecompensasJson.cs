using System.Text.Encodings.Web;
using System.Text.Json;
using System.Text.Json.Serialization;
using Tarjetas.Dominio.Recompensas;

namespace Tarjetas.Infraestructura;

/// <summary>Todas las recompensas en un único archivo pequeño, aparte de los mazos.</summary>
public sealed class RepositorioDeRecompensasJson(string rutaArchivo) : IRecompensasRepositorio
{
    private static readonly JsonSerializerOptions Opciones = new()
    {
        DefaultIgnoreCondition = JsonIgnoreCondition.WhenWritingDefault,
        Encoder = JavaScriptEncoder.UnsafeRelaxedJsonEscaping
    };

    private readonly SemaphoreSlim cerrojo = new(1, 1);
    private Dictionary<Guid, RecompensasDelMazo>? registros;

    public Task<RecompensasDelMazo> ObtenerAsync(Guid mazoId, CancellationToken ct = default) =>
        ConCerrojoAsync(r => Task.FromResult(r.TryGetValue(mazoId, out var existente) ? existente : RecompensasDelMazo.Nuevas(mazoId)), ct);

    public Task GuardarAsync(RecompensasDelMazo recompensas, CancellationToken ct = default) =>
        ConCerrojoAsync(async r =>
        {
            r[recompensas.MazoId] = recompensas;
            await EscribirAsync(r, ct);
            return true;
        }, ct);

    public Task EliminarAsync(Guid mazoId, CancellationToken ct = default) =>
        ConCerrojoAsync(async r =>
        {
            if (r.Remove(mazoId)) await EscribirAsync(r, ct);
            return true;
        }, ct);

    private async Task<T> ConCerrojoAsync<T>(Func<Dictionary<Guid, RecompensasDelMazo>, Task<T>> accion, CancellationToken ct)
    {
        await cerrojo.WaitAsync(ct);
        try
        {
            registros ??= await CargarAsync(ct);
            return await accion(registros);
        }
        finally
        {
            cerrojo.Release();
        }
    }

    private async Task<Dictionary<Guid, RecompensasDelMazo>> CargarAsync(CancellationToken ct)
    {
        if (!File.Exists(rutaArchivo)) return [];
        try
        {
            await using var flujo = File.OpenRead(rutaArchivo);
            var lista = await JsonSerializer.DeserializeAsync<List<RecompensaRegistro>>(flujo, Opciones, ct) ?? [];
            return lista.Select(r => r.ADominio()).ToDictionary(r => r.MazoId);
        }
        catch (JsonException)
        {
            return [];
        }
    }

    private async Task EscribirAsync(Dictionary<Guid, RecompensasDelMazo> actuales, CancellationToken ct)
    {
        Directory.CreateDirectory(Path.GetDirectoryName(rutaArchivo)!);
        var temporal = rutaArchivo + ".tmp";
        await using (var flujo = File.Create(temporal))
        {
            await JsonSerializer.SerializeAsync(flujo, actuales.Values.Select(RecompensaRegistro.De).ToList(), Opciones, ct);
        }
        File.Move(temporal, rutaArchivo, overwrite: true);
    }
}

/// <summary>Formato compacto: cada examen del historial es [fecha unix, total, aciertos, puntos].</summary>
internal sealed record RecompensaRegistro(
    [property: JsonPropertyName("m")] Guid MazoId,
    [property: JsonPropertyName("p")] int Puntos,
    [property: JsonPropertyName("t")] int Totales,
    [property: JsonPropertyName("x")] int Exitosos,
    [property: JsonPropertyName("h")] long[][]? Historial)
{
    public static RecompensaRegistro De(RecompensasDelMazo r) => new(
        r.MazoId, r.Puntos, r.ExamenesTotales, r.ExamenesExitosos,
        r.Historial.Count == 0
            ? null
            : r.Historial.Select(h => new[]
            {
                new DateTimeOffset(DateTime.SpecifyKind(h.FechaUtc, DateTimeKind.Utc)).ToUnixTimeSeconds(),
                h.Total, h.Aciertos, h.Puntos
            }).ToArray());

    public RecompensasDelMazo ADominio() => RecompensasDelMazo.Rehidratar(
        MazoId, Puntos, Totales, Exitosos,
        (Historial ?? []).Select(h => new ExamenRealizado(
            DateTimeOffset.FromUnixTimeSeconds(h[0]).UtcDateTime, (int)h[1], (int)h[2], (int)h[3])));
}

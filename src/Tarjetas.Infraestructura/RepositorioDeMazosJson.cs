using System.Text.Json;
using System.Text.Json.Serialization;
using Tarjetas.Dominio;

namespace Tarjetas.Infraestructura;

/// <summary>
/// Un archivo compacto por mazo (<c>{id}.json</c>) dentro de una carpeta. Al abrir la app se leen una vez a memoria;
/// al guardar solo se reescribe el mazo modificado, de forma atómica (temporal + reemplazo).
/// </summary>
public sealed class RepositorioDeMazosJson(string carpeta) : IMazoRepositorio
{
    private static readonly JsonSerializerOptions Opciones = new()
    {
        DefaultIgnoreCondition = JsonIgnoreCondition.WhenWritingDefault
    };

    private readonly SemaphoreSlim cerrojo = new(1, 1);
    private Dictionary<Guid, Mazo>? mazos;

    public Task<IReadOnlyList<Mazo>> ListarAsync(CancellationToken ct = default) =>
        ConCerrojoAsync(m => Task.FromResult<IReadOnlyList<Mazo>>(m.Values.OrderByDescending(x => x.CreadoUtc).ToList()), ct);

    public Task<Mazo?> ObtenerAsync(Guid id, CancellationToken ct = default) =>
        ConCerrojoAsync(m => Task.FromResult(m.GetValueOrDefault(id)), ct);

    public Task GuardarAsync(Mazo mazo, CancellationToken ct = default) =>
        ConCerrojoAsync(async m =>
        {
            m[mazo.Id] = mazo;
            await EscribirAsync(mazo, ct);
            return true;
        }, ct);

    public Task EliminarAsync(Guid id, CancellationToken ct = default) =>
        ConCerrojoAsync(m =>
        {
            if (m.Remove(id)) File.Delete(RutaDe(id));
            return Task.FromResult(true);
        }, ct);

    private string RutaDe(Guid id) => Path.Combine(carpeta, $"{id:N}.json");

    private async Task<T> ConCerrojoAsync<T>(Func<Dictionary<Guid, Mazo>, Task<T>> accion, CancellationToken ct)
    {
        await cerrojo.WaitAsync(ct);
        try
        {
            mazos ??= await CargarAsync(ct);
            return await accion(mazos);
        }
        finally
        {
            cerrojo.Release();
        }
    }

    private async Task<Dictionary<Guid, Mazo>> CargarAsync(CancellationToken ct)
    {
        Directory.CreateDirectory(carpeta);
        var resultado = new Dictionary<Guid, Mazo>();
        foreach (var archivo in Directory.EnumerateFiles(carpeta, "*.json"))
        {
            try
            {
                await using var flujo = File.OpenRead(archivo);
                var registro = await JsonSerializer.DeserializeAsync<MazoRegistro>(flujo, Opciones, ct);
                if (registro is null) continue;
                var mazo = registro.ADominio();
                resultado[mazo.Id] = mazo;
            }
            catch (JsonException)
            {
                // Un archivo dañado no debe impedir abrir la app: se ignora ese mazo.
            }
        }
        return resultado;
    }

    private async Task EscribirAsync(Mazo mazo, CancellationToken ct)
    {
        Directory.CreateDirectory(carpeta);
        var ruta = RutaDe(mazo.Id);
        var temporal = ruta + ".tmp";
        await using (var flujo = File.Create(temporal))
        {
            await JsonSerializer.SerializeAsync(flujo, MazoRegistro.De(mazo), Opciones, ct);
        }
        File.Move(temporal, ruta, overwrite: true);
    }
}

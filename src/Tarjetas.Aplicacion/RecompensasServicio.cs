using Tarjetas.Dominio;
using Tarjetas.Dominio.Recompensas;

namespace Tarjetas.Aplicacion;

public sealed record PuntosGanados(int Puntos, bool Exitoso);

public sealed record RecompensaMazoVista(
    Guid MazoId, string Nombre, ColorMazo Color, int Puntos, RangoDeMazo Rango,
    int ExamenesExitosos, int ExamenesTotales);

/// <summary>Casos de uso de las recompensas por mazo.</summary>
public sealed class RecompensasServicio(IMazoRepositorio mazos, IRecompensasRepositorio recompensas, TimeProvider reloj)
{
    public async Task<PuntosGanados> RegistrarExamenAsync(Guid mazoId, int total, int aciertos, CancellationToken ct = default)
    {
        var resultado = new ResultadoDeExamen(total, aciertos);
        var registro = await recompensas.ObtenerAsync(mazoId, ct);
        var puntos = registro.RegistrarExamen(resultado, reloj.GetUtcNow().UtcDateTime);
        await recompensas.GuardarAsync(registro, ct);
        return new PuntosGanados(puntos, resultado.EsExitoso);
    }

    public async Task<IReadOnlyList<RecompensaMazoVista>> ListarAsync(CancellationToken ct = default)
    {
        var lista = new List<RecompensaMazoVista>();
        foreach (var mazo in await mazos.ListarAsync(ct))
        {
            var r = await recompensas.ObtenerAsync(mazo.Id, ct);
            lista.Add(new RecompensaMazoVista(mazo.Id, mazo.Nombre, mazo.Color, r.Puntos, r.Rango,
                r.ExamenesExitosos, r.ExamenesTotales));
        }
        return lista.OrderByDescending(x => x.Puntos).ThenBy(x => x.Nombre).ToList();
    }
}

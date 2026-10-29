namespace Tarjetas.Dominio.Recompensas;

public interface IRecompensasRepositorio
{
    /// <summary>Devuelve las recompensas del mazo, o unas vacías (sin guardar) si todavía no tiene.</summary>
    Task<RecompensasDelMazo> ObtenerAsync(Guid mazoId, CancellationToken ct = default);
    Task GuardarAsync(RecompensasDelMazo recompensas, CancellationToken ct = default);
    Task EliminarAsync(Guid mazoId, CancellationToken ct = default);
}

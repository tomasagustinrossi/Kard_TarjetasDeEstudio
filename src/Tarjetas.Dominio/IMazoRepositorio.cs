namespace Tarjetas.Dominio;

/// <summary>Puerto de persistencia: el dominio lo define, la infraestructura lo implementa.</summary>
public interface IMazoRepositorio
{
    Task<IReadOnlyList<Mazo>> ListarAsync(CancellationToken ct = default);
    Task<Mazo?> ObtenerAsync(Guid id, CancellationToken ct = default);
    Task GuardarAsync(Mazo mazo, CancellationToken ct = default);
    Task EliminarAsync(Guid id, CancellationToken ct = default);
}

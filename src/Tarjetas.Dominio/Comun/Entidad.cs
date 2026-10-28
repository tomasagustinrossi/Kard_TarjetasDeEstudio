namespace Tarjetas.Dominio.Comun;

/// <summary>Base de las entidades: se identifican por su Id, no por sus valores.</summary>
public abstract class Entidad
{
    public Guid Id { get; protected set; }
}

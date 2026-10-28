using Tarjetas.Dominio.Comun;

namespace Tarjetas.Dominio;

/// <summary>Raíz de agregado: un grupo de tarjetas con nombre y color. Todo cambio en sus tarjetas pasa por aquí.</summary>
public sealed class Mazo : Entidad
{
    public const int LargoMaximoNombre = 60;
    public const int MaximoTarjetas = 5000;

    private readonly List<Tarjeta> tarjetas;

    public string Nombre { get; private set; }
    public ColorMazo Color { get; private set; }
    public DateTime CreadoUtc { get; }
    public IReadOnlyList<Tarjeta> Tarjetas => tarjetas;

    private Mazo(Guid id, string nombre, ColorMazo color, DateTime creadoUtc, IEnumerable<Tarjeta> existentes)
    {
        Id = id;
        Nombre = nombre;
        Color = color;
        CreadoUtc = creadoUtc;
        tarjetas = [.. existentes];
    }

    public static Mazo Crear(string nombre, ColorMazo color, DateTime ahoraUtc) =>
        new(Guid.NewGuid(), ValidarNombre(nombre), color, ahoraUtc, []);

    public static Mazo Rehidratar(Guid id, string nombre, ColorMazo color, DateTime creadoUtc, IEnumerable<Tarjeta> tarjetas) =>
        new(id, nombre, color, creadoUtc, tarjetas);

    public void Editar(string nombre, ColorMazo color)
    {
        var nombreValido = ValidarNombre(nombre);
        Nombre = nombreValido;
        Color = color;
    }

    /// <summary>Agrega todas o ninguna (una importación inválida no deja el mazo a medias).</summary>
    public void AgregarTarjetas(IReadOnlyCollection<Tarjeta> nuevas)
    {
        if (tarjetas.Count + nuevas.Count > MaximoTarjetas)
            throw new ExcepcionDeDominio($"Un mazo admite hasta {MaximoTarjetas} tarjetas.");
        tarjetas.AddRange(nuevas);
    }

    public Tarjeta ObtenerTarjeta(Guid id) =>
        tarjetas.FirstOrDefault(t => t.Id == id) ?? throw new ExcepcionDeDominio("La tarjeta ya no existe.");

    public void QuitarTarjeta(Guid id) => tarjetas.Remove(ObtenerTarjeta(id));

    /// <summary>Tarjetas que tocan hoy: primero las que llevan más tiempo esperando.</summary>
    public IReadOnlyList<Tarjeta> Pendientes(DateOnly hoy) =>
        tarjetas.Where(t => t.DebeRepasarse(hoy)).OrderBy(t => t.ProximoRepaso(hoy)).ToList();

    private static string ValidarNombre(string? nombre)
    {
        var limpio = nombre?.Trim() ?? "";
        if (limpio.Length == 0) throw new ExcepcionDeDominio("El mazo necesita un nombre.");
        if (limpio.Length > LargoMaximoNombre)
            throw new ExcepcionDeDominio($"El nombre no puede superar los {LargoMaximoNombre} caracteres.");
        return limpio;
    }
}

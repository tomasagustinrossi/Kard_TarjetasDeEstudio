using Tarjetas.Dominio;

namespace Tarjetas.Aplicacion;

/// <summary>Mazo completo con sus tarjetas (las más nuevas primero, como en Keep).</summary>
public sealed record MazoVista(Guid Id, string Nombre, ColorMazo Color, IReadOnlyList<TarjetaVista> Tarjetas)
{
    public int Pendientes => Tarjetas.Count(t => t.DebeRepasarse);

    internal static MazoVista De(Mazo m, DateOnly hoy) => new(
        m.Id, m.Nombre, m.Color, m.Tarjetas.Reverse().Select(t => TarjetaVista.De(t, hoy)).ToList());
}

/// <summary>Resumen liviano para la pantalla principal.</summary>
public sealed record MazoResumen(Guid Id, string Nombre, ColorMazo Color, int Total, int Pendientes);

/// <summary>Las tarjetas que tocan repasar hoy en un mazo.</summary>
public sealed record SesionDeRepaso(Guid MazoId, string Nombre, ColorMazo Color, IReadOnlyList<TarjetaVista> Tarjetas);

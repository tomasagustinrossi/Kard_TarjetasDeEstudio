using Tarjetas.Dominio.Comun;

namespace Tarjetas.Dominio.Recompensas;

/// <summary>
/// Objeto de valor: el resultado de una sesión de repaso ("examen"). Define cuándo es exitoso y cuántos puntos vale.
/// </summary>
public readonly record struct ResultadoDeExamen
{
    public const int PorcentajeMinimoDeExito = 70;
    public const int PuntosPorAcierto = 10;
    public const int BonusPorExamenPerfecto = 50;

    public int Total { get; }
    public int Aciertos { get; }

    public ResultadoDeExamen(int total, int aciertos)
    {
        if (total <= 0 || aciertos < 0 || aciertos > total)
            throw new ExcepcionDeDominio("El resultado del examen no es válido.");
        Total = total;
        Aciertos = aciertos;
    }

    public bool EsExitoso => Aciertos * 100 >= Total * PorcentajeMinimoDeExito;
    public bool EsPerfecto => Aciertos == Total;

    /// <summary>Solo los exámenes exitosos dan puntos: 10 por acierto, más 50 si fue perfecto.</summary>
    public int Puntos => EsExitoso ? Aciertos * PuntosPorAcierto + (EsPerfecto ? BonusPorExamenPerfecto : 0) : 0;
}

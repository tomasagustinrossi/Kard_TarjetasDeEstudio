using Tarjetas.Dominio.Comun;

namespace Tarjetas.Dominio.Recompensas;

/// <summary>
/// Raíz de agregado independiente del mazo (se relaciona solo por su Id): puntos, rango e historial de exámenes.
/// Vive aparte para que el mazo siga siendo liviano y las recompensas no se vean hasta que se pidan.
/// </summary>
public sealed class RecompensasDelMazo : Entidad
{
    public const int MaximoHistorial = 30;

    private readonly List<ExamenRealizado> historial;

    public Guid MazoId => Id;
    public int Puntos { get; private set; }
    public int ExamenesTotales { get; private set; }
    public int ExamenesExitosos { get; private set; }
    public IReadOnlyList<ExamenRealizado> Historial => historial;

    public RangoDeMazo Rango => Puntos switch
    {
        >= 1500 => RangoDeMazo.Maestro,
        >= 500 => RangoDeMazo.Experto,
        >= 100 => RangoDeMazo.Aprendiz,
        _ => RangoDeMazo.Novato
    };

    public ExamenRealizado? UltimoExamen => historial.Count == 0 ? null : historial[^1];

    private RecompensasDelMazo(Guid mazoId, int puntos, int totales, int exitosos, IEnumerable<ExamenRealizado> examenes)
    {
        Id = mazoId;
        Puntos = puntos;
        ExamenesTotales = totales;
        ExamenesExitosos = exitosos;
        historial = [.. examenes];
    }

    public static RecompensasDelMazo Nuevas(Guid mazoId) => new(mazoId, 0, 0, 0, []);

    public static RecompensasDelMazo Rehidratar(Guid mazoId, int puntos, int totales, int exitosos,
        IEnumerable<ExamenRealizado> examenes) => new(mazoId, puntos, totales, exitosos, examenes);

    /// <summary>Registra un examen y devuelve los puntos ganados (0 si no fue exitoso).</summary>
    public int RegistrarExamen(ResultadoDeExamen resultado, DateTime ahoraUtc)
    {
        ExamenesTotales++;
        if (resultado.EsExitoso) ExamenesExitosos++;
        Puntos += resultado.Puntos;

        historial.Add(new ExamenRealizado(ahoraUtc, resultado.Total, resultado.Aciertos, resultado.Puntos));
        if (historial.Count > MaximoHistorial) historial.RemoveAt(0);
        return resultado.Puntos;
    }
}

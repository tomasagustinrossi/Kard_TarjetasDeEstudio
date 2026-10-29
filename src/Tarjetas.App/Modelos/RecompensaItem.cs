using Tarjetas.Aplicacion;
using Tarjetas.Dominio.Recompensas;

namespace Tarjetas.App.Modelos;

public sealed record RecompensaItem(
    Guid MazoId, string Nombre, string Puntos, string Rango, string Detalle, Color Fondo, Brush Borde)
{
    public static RecompensaItem De(RecompensaMazoVista r) => new(
        r.MazoId,
        r.Nombre,
        $"{r.Puntos} pts",
        r.Rango switch
        {
            RangoDeMazo.Maestro => "🏆 Maestro",
            RangoDeMazo.Experto => "🏅 Experto",
            RangoDeMazo.Aprendiz => "⭐ Aprendiz",
            _ => "🌱 Novato"
        },
        r.ExamenesTotales == 0
            ? "Todavía sin exámenes"
            : $"{r.ExamenesExitosos} de {r.ExamenesTotales} exámenes exitosos",
        PaletaKeep.Fondo(r.Color),
        PaletaKeep.Pincel(PaletaKeep.Borde(r.Color)));
}

namespace Tarjetas.Dominio.Repasos;

/// <summary>Escala progresiva de repaso: cada acierto sube un nivel y alarga el intervalo.</summary>
public static class EscalaDeRepaso
{
    private static readonly int[] Intervalos = [1, 3, 7, 14, 30, 70, 140];

    public static int UltimoNivel => Intervalos.Length - 1;

    public static int DiasDelNivel(int nivel) => Intervalos[Math.Clamp(nivel, 0, UltimoNivel)];
}

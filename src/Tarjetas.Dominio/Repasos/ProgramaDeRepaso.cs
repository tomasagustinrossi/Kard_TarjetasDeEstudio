namespace Tarjetas.Dominio.Repasos;

/// <summary>
/// Objeto de valor inmutable: en qué nivel de la escala está una tarjeta y cuándo se repasó por última vez.
/// Toda la lógica del repaso espaciado (y el tope de la deuda) vive aquí.
/// </summary>
public readonly record struct ProgramaDeRepaso(int Nivel, DateOnly? UltimoRepaso)
{
    public static ProgramaDeRepaso Nuevo => new(0, null);

    public bool EsNueva => UltimoRepaso is null;

    public int IntervaloEnDias => EscalaDeRepaso.DiasDelNivel(Nivel);

    /// <summary>Máximo de días de atraso que se muestran: intervalo×2 − intervalo/2 (70 días → 140 − 35 = 105).</summary>
    public int LimiteDeDeuda => IntervaloEnDias * 2 - IntervaloEnDias / 2;

    public DateOnly ProximoRepaso(DateOnly hoy) => UltimoRepaso?.AddDays(IntervaloEnDias) ?? hoy;

    public bool DebeRepasarse(DateOnly hoy) => hoy >= ProximoRepaso(hoy);

    public int DiasDeAtraso(DateOnly hoy) => Math.Max(0, hoy.DayNumber - ProximoRepaso(hoy).DayNumber);

    /// <summary>Días de deuda a mostrar; null si no hay atraso o si ya superó el tope (la etiqueta desaparece).</summary>
    public int? DeudaVisible(DateOnly hoy)
    {
        var atraso = DiasDeAtraso(hoy);
        return atraso > 0 && atraso <= LimiteDeDeuda ? atraso : null;
    }

    public ProgramaDeRepaso Recordada(DateOnly hoy) =>
        new(EsNueva ? 0 : Math.Min(Nivel + 1, EscalaDeRepaso.UltimoNivel), hoy);

    public ProgramaDeRepaso Olvidada(DateOnly hoy) => new(0, hoy);
}

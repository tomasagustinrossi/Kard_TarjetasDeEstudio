using Tarjetas.Dominio;

namespace Tarjetas.App.Modelos;

/// <summary>Colores de nota de Google Keep (tema claro y oscuro) y colores de apoyo de la interfaz.</summary>
public static class PaletaKeep
{
    private static readonly Dictionary<ColorMazo, (string Claro, string Oscuro)> Colores = new()
    {
        [ColorMazo.Predeterminado] = ("#FFFFFF", "#202124"),
        [ColorMazo.Coral] = ("#FAAFA8", "#77172E"),
        [ColorMazo.Durazno] = ("#F39F76", "#692B17"),
        [ColorMazo.Arena] = ("#FFF8B8", "#7C4A03"),
        [ColorMazo.Menta] = ("#E2F6D3", "#264D3B"),
        [ColorMazo.Salvia] = ("#B4DDD3", "#0C625D"),
        [ColorMazo.Niebla] = ("#D4E4ED", "#256377"),
        [ColorMazo.Tormenta] = ("#AECCDC", "#284255"),
        [ColorMazo.Crepusculo] = ("#D3BFDB", "#472E5B"),
        [ColorMazo.Flor] = ("#F6E2DD", "#6C394F"),
        [ColorMazo.Arcilla] = ("#E9E3D4", "#4B443A"),
        [ColorMazo.Tiza] = ("#EFEFF1", "#232427")
    };

    public static bool EsOscuro => Application.Current?.RequestedTheme == AppTheme.Dark;

    public static IReadOnlyList<ColorMazo> Todos { get; } = Enum.GetValues<ColorMazo>();

    public static Color Fondo(ColorMazo color)
    {
        var (claro, oscuro) = Colores[color];
        return Color.FromArgb(EsOscuro ? oscuro : claro);
    }

    public static Color Borde(ColorMazo color) =>
        color == ColorMazo.Predeterminado ? Color.FromArgb(EsOscuro ? "#5F6368" : "#E0E0E0") : Fondo(color);

    public static Brush Pincel(Color color) => new SolidColorBrush(color);

    public static Color Superficie => Color.FromArgb(EsOscuro ? "#303134" : "#F1F3F4");
    public static Color FondoEtiqueta => Color.FromArgb(EsOscuro ? "#33FFFFFF" : "#14000000");
    public static Color Seleccion => Color.FromArgb(EsOscuro ? "#8AB4F8" : "#1A73E8");
    public static Color FondoAcierto => Color.FromArgb(EsOscuro ? "#1E4620" : "#CEEAD6");
    public static Color FondoError => Color.FromArgb(EsOscuro ? "#5C1A17" : "#FAD2CF");
    public static Color TextoAcierto => Color.FromArgb(EsOscuro ? "#81C995" : "#1E8E3E");
    public static Color TextoError => Color.FromArgb(EsOscuro ? "#F28B82" : "#D93025");
}
